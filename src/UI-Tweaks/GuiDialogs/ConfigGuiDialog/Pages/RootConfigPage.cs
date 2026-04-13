using BitzArt.UI.Tweaks.Config;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal sealed class RootConfigPage(UiTweaksModConfig config) : ConfigPage(Constants.ModName)
{
    private const int NavButtonWidth = 200;
    private const int NavButtonHeight = 28;
    private const int NavButtonGap = 4;

    public override void ComposeContent(ICoreClientAPI clientApi, GuiComposer composer, ElementBounds bounds, Action saveConfig, Action<ConfigPage> pushPage)
    {
        double buttonX = bounds.fixedX + (bounds.fixedWidth - NavButtonWidth) / 2.0;

        var hudButtonBounds = ElementBounds.Fixed(
            buttonX,
            bounds.fixedY,
            NavButtonWidth,
            NavButtonHeight);

        var quickSearchButtonBounds = ElementBounds.Fixed(
            buttonX,
            bounds.fixedY + NavButtonHeight + NavButtonGap,
            NavButtonWidth,
            NavButtonHeight);

        composer
            .AddSmallButton(
                Lang.Get($"{Constants.ModId}:config-page-hud"),
                () => { pushPage(new HudConfigPage(config.Hud)); return true; },
                hudButtonBounds,
                key: "nav-hud")
            .AddSmallButton(
                Lang.Get($"{Constants.ModId}:config-page-quicksearch"),
                () => { pushPage(new QuickSearchConfigPage(config.QuickSearch)); return true; },
                quickSearchButtonBounds,
                key: "nav-quicksearch");
    }
}
