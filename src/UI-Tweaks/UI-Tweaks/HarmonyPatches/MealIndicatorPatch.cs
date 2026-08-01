using Cairo;
using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace BitzArt.UI.Tweaks;

internal static class MealIndicatorPatch
{
    private const string ComposeSlotOverlaysMethodName = "ComposeSlotOverlays";

    public static void Patch(Harmony harmony)
    {
        var original = GetComposeSlotOverlaysMethod();
        var postfix = AccessTools.Method(typeof(MealIndicatorPatch), nameof(Postfix));

        harmony.Patch(original, postfix: new HarmonyMethod(postfix));
    }

    public static void Unpatch(Harmony harmony)
    {
        var original = GetComposeSlotOverlaysMethod();

        harmony.Unpatch(original, HarmonyPatchType.Postfix, harmony.Id);
    }

    private static MethodInfo GetComposeSlotOverlaysMethod()
    {
        return AccessTools.DeclaredMethod(
            typeof(GuiElementItemSlotGridBase),
            ComposeSlotOverlaysMethodName,
            [typeof(ItemSlot), typeof(int), typeof(int)]);
    }

    private static void Postfix(
        GuiElementItemSlotGridBase __instance,
        ItemSlot slot,
        int slotIndex,
        bool __result)
    {
        if (!__result
            || slot.Inventory?.Api is not ICoreClientAPI clientApi
            || !TryGetRemainingFraction(slot.Itemstack, clientApi.World, out var remainingFraction))
        {
            return;
        }

        ComposeIndicator(__instance, slotIndex, clientApi, remainingFraction);
    }

    private static bool TryGetRemainingFraction(
        ItemStack? stack,
        IWorldAccessor world,
        out float remainingFraction)
    {
        remainingFraction = 0;

        if (stack?.Block is not BlockMeal mealBlock
            || !mealBlock.FirstCodePart().Contains("bowl", StringComparison.Ordinal))
        {
            return false;
        }

        var servingCapacity = mealBlock.Attributes?["servingCapacity"].AsFloat(1) ?? 1;
        var remainingServings = mealBlock.GetQuantityServings(world, stack);

        if (servingCapacity <= 0
            || remainingServings <= 0
            || remainingServings >= servingCapacity)
        {
            return false;
        }

        remainingFraction = remainingServings / servingCapacity;
        return true;
    }

    private static void ComposeIndicator(
        GuiElementItemSlotGridBase slotGrid,
        int slotIndex,
        ICoreClientAPI clientApi,
        float remainingFraction)
    {
        var bounds = slotGrid.SlotBounds[slotIndex];
        using var surface = new ImageSurface(
            Format.Argb32,
            (int)bounds.InnerWidth,
            (int)bounds.InnerHeight);
        using var context = GuiElement.GenContext(surface);

        var x = GuiElement.scaled(4);
        var y = (int)bounds.InnerHeight - GuiElement.scaled(7);
        var fullWidth = bounds.InnerWidth - GuiElement.scaled(8);
        var height = GuiElement.scaled(4);

        context.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
        DrawBar(slotGrid, context, x, y, fullWidth, height);

        var colorIndex = Math.Clamp((int)(remainingFraction * 100), 0, 99);
        var color = ColorUtil.ToRGBAFloats(GuiStyle.DamageColorGradient[colorIndex]);

        context.SetSourceRGB(color[0], color[1], color[2]);
        DrawBar(slotGrid, context, x, y, remainingFraction * fullWidth, height);

        clientApi.Gui.LoadOrUpdateCairoTexture(
            surface,
            true,
            ref slotGrid.slotQuantityTextures[slotIndex]);
    }

    private static void DrawBar(
        GuiElementItemSlotGridBase slotGrid,
        Context context,
        double x,
        double y,
        double width,
        double height)
    {
        GuiElement.RoundRectangle(context, x, y, width, height, 1);
        context.FillPreserve();
        slotGrid.ShadePath(context, 2);
    }
}
