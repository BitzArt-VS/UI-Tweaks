using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

// This ModSystem initializes the StatusHud functionality when the client starts,
// and disposes of it when the ModSystem is unloaded.
public class StatusHudModSystem : ClientModSystem
{
    protected override string Name => $"{Constants.ModName}:StatusHUD";

    private HealthbarTooltip? _healthbarTooltip;
    private SatietyTooltip? _satietyTooltip;

    protected override void Start(ICoreClientAPI clientApi)
    {
        var config = clientApi.GetModConfig<UiTweaksModConfig>(Constants.ModConfigFileName).Hud;

        if (config.HealthbarTooltip.Enable)
        {
            _healthbarTooltip = new(clientApi, config.HealthbarTooltip);
        }

        if (config.SatietyTooltip.Enable)
        {
            _satietyTooltip = new(clientApi, config.SatietyTooltip);
        }
    }

    public override void Dispose()
    {
        _healthbarTooltip?.Dispose();
        _healthbarTooltip = null;

        _satietyTooltip?.Dispose();
        _satietyTooltip = null;
    }
}
