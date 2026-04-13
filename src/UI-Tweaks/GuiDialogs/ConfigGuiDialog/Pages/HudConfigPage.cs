using BitzArt.UI.Tweaks.Config;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal class HudConfigPage(HudConfig config) : ConfigPage(Lang.Get($"{Constants.ModId}:config-page-hud"))
{
    private readonly HudConfig _config = config;

    public override void ComposeContent(ICoreClientAPI clientApi, GuiComposer composer, ElementBounds bounds, Action saveConfig, Action<ConfigPage> pushPage)
    {
        // TODO: HUD configuration controls
    }
}
