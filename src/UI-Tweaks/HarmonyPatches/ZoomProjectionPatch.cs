using HarmonyLib;
using System;
using Vintagestory.Client.NoObf;

namespace BitzArt.UI.Tweaks;

[HarmonyPatch(typeof(ClientMain), nameof(ClientMain.Set3DProjection), [typeof(float), typeof(float)])]
internal static class ZoomProjectionPatch
{
    private const float RadiansToDegrees = 180 / MathF.PI;

    private static void Prefix(ClientMain __instance, ref float fov)
    {
        float fieldOfViewFactor = ZoomRuntimeState.FieldOfViewFactor;

        if (fieldOfViewFactor >= 0.999f)
        {
            return;
        }

        fov *= fieldOfViewFactor;

        float fieldOfViewDegrees = fov * RadiansToDegrees;
        __instance.MainCamera.ZNear = Math.Clamp(0.1f - fieldOfViewDegrees / 90f / 25f, 0.025f, 0.1f);
    }
}
