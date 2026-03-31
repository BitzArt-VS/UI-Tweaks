using BitzArt.UI.Tweaks.Services;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal class SatietyTooltipLabel(ICoreClientAPI clientApi, GameStatusService statusService, IHudTooltipConfiguration config)
    : HudLabel(clientApi, statusService, config)
{
    protected override void OnInitialize()
    {
        DialogArea = EnumDialogArea.CenterBottom;

        Offset.X += 252.0;
        Offset.Y += -83.0;

        FormatReplacements =
        [
            new("current", "player-satiety-current"),
            new("maximum", "player-satiety-max"),
            new("percent", "player-satiety-percent"),
            new("hunger", "player-satiety-hunger")
        ];
    }
}
