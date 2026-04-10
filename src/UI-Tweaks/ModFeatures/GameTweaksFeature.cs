using BitzArt.UI.Tweaks.Config;
using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

public class GameTweaksFeature(UiTweaksModSystem modSystem, UiTweaksModConfig config)
    : ModSystemFeature<UiTweaksModSystem, UiTweaksModConfig>(modSystem, config)
{
    private Harmony? _harmony;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void Start(ICoreClientAPI clientApi)
    {
        _harmony = new Harmony(Constants.ModId);
        _harmony.PatchAll(typeof(UiTweaksModSystem).Assembly);
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(_harmony.Id);
        _harmony = null;

        GC.SuppressFinalize(this);
    }
}
