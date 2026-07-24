namespace BitzArt.UI.Tweaks.Gui;

internal sealed class GuiNodeSlot(
    GuiSurfaceRenderer renderer,
    GuiSlot? parent,
    IGuiNode node,
    GuiTreeBuilder childTreeBuilder,
    GuiTreeBuilder.TreeFrame frame)
    : GuiSlot(renderer, parent, node, childTreeBuilder, frame)
{
    public override bool IsArranging => false;

    public override void Arrange()
    {
        throw new InvalidOperationException(
            "A layout-transparent node cannot be arranged as a component.");
    }
}
