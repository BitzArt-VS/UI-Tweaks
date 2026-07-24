namespace BitzArt.UI.Tweaks.Gui;

internal sealed class GuiNodeSlot(
    GuiSurfaceRenderer renderer,
    GuiSlot? parent,
    IGuiNode node,
    GuiTreeBuilder childTreeBuilder,
    GuiTreeBuilder.TreeFrame frame)
    : GuiSlot(renderer, parent, node, childTreeBuilder, frame)
{
    public override void Arrange()
    {
        if (IsArranging)
        {
            throw new InvalidOperationException(
                $"Arrangement cycle detected at {Instance.GetType().Name}.");
        }

        IsArranging = true;
        try
        {
            foreach (GuiSlot child in Children)
            {
                child.Arrange();
            }
        }
        finally
        {
            IsArranging = false;
        }
    }
}
