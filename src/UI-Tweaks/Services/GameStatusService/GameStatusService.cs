using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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

    private CultureInfo _formatCulture;

    private long? _tickListenerId;
    private CancellationTokenSource? _updateThreadCts;
    private Thread? _updateThread;
    private EntityPlayer? _playerEntity;

    private readonly SubscriptionCollection _subscriptions;
    private readonly DetailRecordCollection _detailRecords;

    public GameStatusService(ICoreClientAPI clientApi)
    {
        _clientApi = clientApi;
        _subscriptions = new();
        _detailRecords = new();

        string[] months = [.. Enumerable.Range(1, 12).Select(i => Lang.Get("month-" + (EnumMonth)i)), string.Empty];

        _formatCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        _formatCulture.DateTimeFormat.MonthNames = months;
        _formatCulture.DateTimeFormat.AbbreviatedMonthNames = [.. months.Select(m => m.Length > 3 ? m[..3] : m)];
        _formatCulture.DateTimeFormat.MonthGenitiveNames = _formatCulture.DateTimeFormat.MonthNames;
        _formatCulture.DateTimeFormat.AbbreviatedMonthGenitiveNames = _formatCulture.DateTimeFormat.AbbreviatedMonthNames;

        InitStats();

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

                        await Task.Delay(1000, _updateThreadCts.Token);
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

    public bool Subscribe(string format, Action<string> callback)
    {
        var placeholderRegex = GetFormatPlaceholderRegex();
        var matches = placeholderRegex.Matches(format);

        // No subscription is necessary if
        // there are no variable placeholders in the format string.
        if (matches.Count == 0)
        {
            return false;
        }

        // Extract just the variable names for your subscription logic
        var variableNames = matches.Select(m => m.Groups["name"].Value).ToList();

        // Replace placeholders with consecutive iterator numbers, preserving formatting
        int i = 0;
        var resultingFormat = placeholderRegex.Replace(format, match =>
        {
            // match.Groups["format"].Value will contain the alignment/format string (e.g. ":MMMM dd, yyyy")
            // If there is no format string, it will be empty, cleanly resulting in just {0}, {1}, etc.
            return $"{{{i++}{match.Groups["format"].Value}}}";
        });

        Subscribe(variableNames, (values) =>
        {
            callback.Invoke(string.Format(_formatCulture, resultingFormat, [.. values]));
        });

        return true;
    }

    // Captures the variable name in the "name" group, and any following alignment/formatting in the "format" group
    [GeneratedRegex(@"\{(?<name>[a-zA-Z0-9\-]+)(?<format>[,:][^}]+)?\}")]
    private static partial Regex GetFormatPlaceholderRegex();

    public void Subscribe(List<GameStatusDetailType> details, Action<object[]> callback)
        => _subscriptions.Subscribe([.. details.Select(_detailRecords.Get)], callback);

    public void Subscribe(List<string> details, Action<object[]> callback)
        => _subscriptions.Subscribe([.. details.Select(_detailRecords.Get)], callback);

    public void Unsubscribe(Action<object[]> callback)
        => _subscriptions.Unsubscribe(callback);
}
