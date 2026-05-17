using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace BitzArt.UI.Tweaks;

internal static class ZoomMouseSensitivityPatch
{
    private readonly record struct MouseDeltaState(double MouseDeltaX, double MouseDeltaY);

    public static void Patch(Harmony harmony)
    {
        var original = AccessTools.Method(typeof(ClientMain), nameof(ClientMain.OnMouseMove));
        var prefix = AccessTools.Method(typeof(ZoomMouseSensitivityPatch), nameof(Prefix));
        var postfix = AccessTools.Method(typeof(ZoomMouseSensitivityPatch), nameof(Postfix));

        harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
    }

    private static void Prefix(ClientMain __instance, out MouseDeltaState __state)
    {
        __state = new(__instance.MouseDeltaX, __instance.MouseDeltaY);
    }

    private static void Postfix(ClientMain __instance, MouseDeltaState __state)
    {
        float mouseSensitivityFactor = ZoomRuntimeState.MouseSensitivityFactor;

        if (mouseSensitivityFactor >= 0.999f)
        {
            return;
        }

        double addedMouseDeltaX = __instance.MouseDeltaX - __state.MouseDeltaX;
        double addedMouseDeltaY = __instance.MouseDeltaY - __state.MouseDeltaY;

        __instance.MouseDeltaX = __state.MouseDeltaX + addedMouseDeltaX * mouseSensitivityFactor;
        __instance.MouseDeltaY = __state.MouseDeltaY + addedMouseDeltaY * mouseSensitivityFactor;
    }
}
