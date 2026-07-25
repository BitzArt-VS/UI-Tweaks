namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Default base class for layout-participating components. Extends <see cref="GuiNode"/>
/// with the <see cref="LayoutParameters"/> bundle and a virtual <see cref="Arrange"/>
/// hook consumed by the layout pass. Pure decorators that do not occupy layout space
/// should inherit from <see cref="GuiNode"/> directly instead.
/// </summary>
public abstract class GuiComponent : GuiNode, IGuiComponent
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    protected GuiComponent()
    {
        LayoutParameters = new GuiComponentLayoutParameters();
    }

    /// <inheritdoc/>
    public virtual GuiComponentBounds Arrange()
    {
        // Arrangement recursively carries provisional constraints down from ancestors,
        // then carries resolved descendant results back up as each call returns. A layout
        // parent's bounds are still provisional during this call, just as this slot's
        // bounds remain provisional until its own Arrange call returns. Fit-content and
        // fill rules can share a top-down constraint envelope while producing different
        // bottom-up results, and dependency-sensitive layouts may involve repeated passes.
        var slot = (GuiComponentSlot)Slot!;

        slot.Bounds = LayoutParameters.ResolveBounds(
            slot.LayoutParent?.Bounds);
        slot.ArrangeChildren();

        return ResolveBounds(slot);
    }

    private GuiComponentBounds ResolveBounds(GuiComponentSlot slot)
        => throw new NotImplementedException();
}
