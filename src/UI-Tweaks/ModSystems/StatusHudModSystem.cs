using BitzArt.UI.Tweaks.Services;
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
            config.HealthbarTooltip,
            config.TemporalStabilityTooltip,
            config.SatietyTooltip,
            config.HungerTooltip,

            ..config.CustomTooltips
        ];

        foreach (var tooltipConfig in tooltipConfigurations)
        {
            _tooltipLabels.Add(new(clientApi, _gameStatusService, tooltipConfig));
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
