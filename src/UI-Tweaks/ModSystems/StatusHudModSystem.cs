using BitzArt.UI.Tweaks.Config;
using BitzArt.UI.Tweaks.Services;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class StatusHudModSystem : ClientModSystem
{
    protected override string Name => $"{Constants.ModName}:StatusHUD";

    private GameStatusService? _gameStatusService;

    private readonly List<HudTooltipLabel> _tooltipLabels = [];

    protected override void Start(ICoreClientAPI clientApi)
    {
        var config = clientApi.GetModConfig<UiTweaksModConfig>(Constants.ModConfigFileName).Hud;
        _gameStatusService = new(clientApi);

        List<IHudTooltipConfiguration> tooltipConfigurations =
        [
            config.EnvironmentWidget,

            config.HealthbarTooltip,
            config.TemporalStabilityTooltip,
            config.SatietyTooltip,
            config.HungerTooltip,

            // Not enabling custom tooltip,
            // it is provided for config demonstration purposes only.
            // config.ExampleCustomTooltip

            ..config.CustomTooltips
        ];

        foreach (var tooltipConfig in tooltipConfigurations)
        {
            try
            {
                _tooltipLabels.Add(new(clientApi, _gameStatusService, tooltipConfig));
            }
            catch (Exception ex)
            {
                clientApi.Logger.Error($"Failed to initialize tooltip '{tooltipConfig.ComponentName}'. It will not be added.");
                clientApi.Logger.Error(ex);
            }
            
        }
    }

    public override void Dispose()
    {
        _gameStatusService?.Dispose();
        _gameStatusService = null;

        for (int i = _tooltipLabels.Count - 1; i >= 0; i--)
        {
            _tooltipLabels[i].Dispose();
            _tooltipLabels.RemoveAt(i);
        }
    }
}
