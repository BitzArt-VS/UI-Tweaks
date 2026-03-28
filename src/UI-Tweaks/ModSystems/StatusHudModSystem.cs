using BitzArt.UI.Tweaks.Services;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

// This ModSystem initializes the StatusHud functionality when the client starts,
// and disposes of it when the ModSystem is unloaded.
public class StatusHudModSystem : ClientModSystem
{
    protected override string Name => $"{Constants.ModName}:StatusHUD";

    private GameStatusService? _gameStatusService;

    private HealthbarTooltip? _healthbarTooltip;
    private SatietyTooltip? _satietyTooltip;

    protected override void Start(ICoreClientAPI clientApi)
    {
        var config = clientApi.GetModConfig<UiTweaksModConfig>(Constants.ModConfigFileName).Hud;
        _gameStatusService = new(clientApi);

        _healthbarTooltip = new(clientApi, _gameStatusService, config.HealthbarTooltip);
        _satietyTooltip = new(clientApi, _gameStatusService, config.SatietyTooltip);
    }

    public override void Dispose()
    {
        _gameStatusService?.Dispose();
        _gameStatusService = null;

        _healthbarTooltip?.Dispose();
        _healthbarTooltip = null;

        _satietyTooltip?.Dispose();
        _satietyTooltip = null;
    }
}
