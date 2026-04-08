using BitzArt.UI.Tweaks.Config;
using BitzArt.UI.Tweaks.Services;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class StatusHudModSystem : ClientModSystem
{
    private readonly List<HudTooltipLabel> _tooltipLabels = [];

    private GameStatusService? _gameStatusService;
    private Harmony? _harmony;

    protected override string Name => $"{Constants.ModName}:StatusHUD";

    protected override void Start(ICoreClientAPI clientApi)
    {
        var config = clientApi.GetModConfig<UiTweaksModConfig>(Constants.ModConfigFileName).Hud;

        _harmony = new Harmony(Constants.ModId);
        _harmony.PatchAll(typeof(StatusHudModSystem).Assembly);

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
        _harmony?.UnpatchAll(_harmony.Id);
        _harmony = null;

        _gameStatusService?.Dispose();
        _gameStatusService = null;

        foreach (var label in _tooltipLabels)
        {
            label.Dispose();
        }

        _tooltipLabels.Clear();
    }
}
