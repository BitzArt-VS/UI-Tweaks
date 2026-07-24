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
        // Plain nodes have no layout responsibility, so there is nothing to arrange.
    }
}
