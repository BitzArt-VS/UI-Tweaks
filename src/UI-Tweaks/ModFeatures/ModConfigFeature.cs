using BitzArt.UI.Tweaks.Config;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

public class ModConfigFeature(UiTweaksModSystem modSystem, UiTweaksModConfig config)
    : ModSystemFeature<UiTweaksModSystem, UiTweaksModConfig>(modSystem, config)
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void Start(ICoreClientAPI clientApi)
    {
        clientApi.Input.AddHotKey(ModHotKeys.ModConfiguration, (keys) => ToggleDialog(clientApi));
    }

    private static bool ToggleDialog(ICoreClientAPI clientApi)
    {
        // TODO: Mod config dialog

        clientApi.ShowChatMessage("Mod configuration dialog is to be implemented.");

        return true;
    }
}
