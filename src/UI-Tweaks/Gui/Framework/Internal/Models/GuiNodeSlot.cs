namespace BitzArt.UI.Tweaks.Gui;

internal sealed class GuiNodeSlot(
    GuiSurfaceRenderer renderer,
    GuiSlot? parent,
    IGuiNode node,
    GuiTreeBuilder childTreeBuilder,
    GuiTreeBuilder.TreeFrame frame)
    : GuiSlot(renderer, parent, node, childTreeBuilder, frame)
{
    private protected override void OnArrange()
        => ArrangeChildren();
}
