using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BitzArt.UI.Tweaks.Services;

/// <summary>
/// A service that aggregates game status updates
/// and allows other components to dynamically subscribe to arbitrary subsets of these updates,
/// allowing updates from different sources in a single callback.
/// </summary>
/// <remarks>
/// This service runs on a separate thread, and will notify subscribers of changes from that thread.
/// Dispatching to the UI thread is necessary when doing any UI updates in the callback.
/// </remarks>
public partial class GameStatusService : IDisposable
{
    private readonly ICoreClientAPI _clientApi;
    private SystemTemporalStability? _temporalStabilitySystem;

    private long? _tickListenerId;
    private CancellationTokenSource? _updateThreadCts;
    private Thread? _updateThread;
    private EntityPlayer? _playerEntity;

    private readonly SubscriptionCollection _subscriptions;
    private readonly DetailRecordCollection _detailRecords;

    private readonly DetailRecord<float> _playerHealthCurrent;
    private readonly DetailRecord<float> _playerHealthMax;
    private readonly DetailRecord<float> _playerHealthPercentage;

    private readonly DetailRecord<float> _playerSatietyCurrent;
    private readonly DetailRecord<float> _playerSatietyMax;
    private readonly DetailRecord<float> _playerSatietyPercent;
    private readonly DetailRecord<float> _playerSatietyHungerRate;

    private readonly DetailRecord<float> _playerTemporalStability;

    private readonly DetailRecord<float> _playerLocationTemporalStability;

    public GameStatusService(ICoreClientAPI clientApi)
    {
        _clientApi = clientApi;
        _subscriptions = new();
        _detailRecords = new();

        _playerHealthCurrent = new(GameStatusDetailType.PlayerCurrentHealth, "player-health-current", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerHealthCurrent);
        _playerHealthMax = new(GameStatusDetailType.PlayerMaxHealth, "player-health-max", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerHealthMax);
        _playerHealthPercentage = new(GameStatusDetailType.PlayerHealthPercentage, "player-health-percent", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerHealthPercentage);

        _playerSatietyCurrent = new(GameStatusDetailType.PlayerCurrentSatiety, "player-satiety-current", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyCurrent);
        _playerSatietyMax = new(GameStatusDetailType.PlayerMaxSatiety, "player-satiety-max", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyMax);
        _playerSatietyPercent = new(GameStatusDetailType.PlayerSatietyPercentage, "player-satiety-percent", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyPercent);
        _playerSatietyHungerRate = new(GameStatusDetailType.PlayerSatietyHungerRate, "player-satiety-hunger", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerSatietyHungerRate);

        _playerTemporalStability = new(GameStatusDetailType.PlayerTemporalStability, "player-temporal-stability", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerTemporalStability);

        _playerLocationTemporalStability = new(GameStatusDetailType.PlayerLocationTemporalStability, "player-location-temporal-stability", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerLocationTemporalStability);

        _tickListenerId = _clientApi.Event.RegisterGameTickListener(_ =>
        {
            _playerEntity = _clientApi.World?.Player?.Entity;

            if (_playerEntity?.WatchedAttributes is null)
            {
                return;
            }

            if (_updateThread is not null)
            {
                _clientApi.Logger.Warning("GameStatusService: Update thread already running.");

                _clientApi.Event.UnregisterGameTickListener(_tickListenerId!.Value);
                return;
            }

            _updateThreadCts = new();
            var token = _updateThreadCts.Token;

            _updateThread = new Thread(async () =>
            {
                _clientApi.Logger.Debug("GameStatusService: Update thread started.");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        UpdateStats();
                        await Task.Delay(50, _updateThreadCts.Token);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        _clientApi.Logger.Error("Error updating game status details: " + ex);
                    }
                }

                _clientApi.Logger.Debug("GameStatusService: Update thread exiting.");
            })
            {
                IsBackground = true,
                Name = "UI-Tweaks Status Updates Thread"
            };

            _updateThread.Start();

            _clientApi.Event.UnregisterGameTickListener(_tickListenerId!.Value);

        }, 100);
    }

    public void Dispose()
    {
        if (_tickListenerId is not null)
        {
            _clientApi.Event.UnregisterGameTickListener(_tickListenerId.Value);
            _tickListenerId = null;
        }

        _updateThreadCts?.Cancel();
        _updateThreadCts = null;

        _updateThread = null;

        GC.SuppressFinalize(this);
    }

    private void UpdateStats()
    {
        if (_playerEntity?.WatchedAttributes is null)
        {
            throw new InvalidOperationException("Player entity or watched attributes not available.");
        }

        var healthAttribute = _playerEntity.WatchedAttributes.GetTreeAttribute("health");

        var healthCurrent = healthAttribute?.TryGetFloat("currenthealth");
        var healthMax = healthAttribute?.TryGetFloat("maxhealth");
        float? healthPercent = healthCurrent.HasValue && healthMax.HasValue && healthMax.Value > 0
            ? healthCurrent.Value / healthMax.Value * 100.0f
            : null;

        var hungerAttribute = _playerEntity.WatchedAttributes.GetTreeAttribute("hunger");

        var satietyCurrent = hungerAttribute?.TryGetFloat("currentsaturation");
        var satietyMax = hungerAttribute?.TryGetFloat("maxsaturation");
        float? satietyPercent = satietyCurrent.HasValue && satietyMax.HasValue && satietyMax.Value > 0
            ? satietyCurrent.Value / satietyMax.Value * 100.0f
            : null;

        var hungerRate = _playerEntity.Stats.GetBlended("hungerrate");

        var playerTemporalStability = (float)_playerEntity.WatchedAttributes.GetDouble("temporalStability");

        _temporalStabilitySystem ??= _clientApi.ModLoader.GetModSystem<SystemTemporalStability>();
        var playerLocationTemporalStability = _temporalStabilitySystem!.GetTemporalStability(_playerEntity.Pos.AsBlockPos);

        Span<DetailRecord<float>> affectedDetails =
        [
            _playerHealthCurrent,
            _playerHealthMax,
            _playerHealthPercentage,
            _playerSatietyCurrent,
            _playerSatietyMax,
            _playerSatietyPercent,
            _playerSatietyHungerRate,
            _playerTemporalStability,
            _playerLocationTemporalStability
        ];

        Span<float?> values =
        [
            healthCurrent,
            healthMax,
            healthPercent,
            satietyCurrent,
            satietyMax,
            satietyPercent,
            hungerRate,
            playerTemporalStability,
            playerLocationTemporalStability
        ];

        List<DetailRecord<float>> updatedDetails = new(affectedDetails.Length);
        for (int i = 0; i < affectedDetails.Length; i++)
        {
            var detail = affectedDetails[i];
            var value = values[i];

            if (value is not null && detail.Update(value!.Value))
            {
                updatedDetails.Add(detail);
            }
        }

        _subscriptions.OnUpdate(updatedDetails);
    }

    public void Subscribe(string format, Action<object[]> callback, out string resultingFormat)
    {
        var placeholderRegex = GetFormatPlaceholderRegex();
        var matches = placeholderRegex.Matches(format).Select(m => m.Groups[1].Value).ToList();
        Subscribe(matches, callback);

        // Replace placeholder with consecutive iterator numbers for string.Format compatibility
        resultingFormat = format;
        for (int i = 0; i < matches.Count; i++)
        {
            resultingFormat = resultingFormat.Replace($"{{{matches[i]}}}", $"{{{i}}}");
        }
    }

    public void Subscribe(List<GameStatusDetailType> details, Action<object[]> callback)
        => _subscriptions.Subscribe([.. details.Select(_detailRecords.Get)], callback);

    public void Subscribe(List<string> details, Action<object[]> callback)
        => _subscriptions.Subscribe([.. details.Select(_detailRecords.Get)], callback);

    public void Unsubscribe(Action<object[]> callback)
        => _subscriptions.Unsubscribe(callback);

    [GeneratedRegex(@"\{([a-zA-Z0-9\-]+)\}")]
    private static partial Regex GetFormatPlaceholderRegex();
}
