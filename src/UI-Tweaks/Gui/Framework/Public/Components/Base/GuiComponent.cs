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
    public virtual GuiBounds Arrange(
        GuiBounds availableBounds)
    {
        // Arrangement recursively carries provisional constraints down from ancestors,
        // then carries resolved descendant results back up as each call returns. A layout
        // parent's bounds are still provisional during this call, just as this slot's
        // bounds remain provisional until its own Arrange call returns. Fit-content and
        // explicit rules can share a top-down constraint envelope while producing different
        // bottom-up results, and dependency-sensitive layouts may involve repeated passes.
        var slot = (GuiComponentSlot)Slot!;

        GuiBounds provisionalBounds =
            LayoutParameters.ResolveBounds(availableBounds);

        slot.Bounds = provisionalBounds;
        slot.ContentBounds =
            provisionalBounds.ToContentBounds();

        GuiBounds? descendantsBounds =
            slot.ArrangeChildren(slot.ContentBounds.Value);

        return ResolveFinalBounds(
            availableBounds,
            descendantsBounds);
    }

    /// <summary>
    /// Resolves this component's final bounds after its descendants have been arranged.
    /// </summary>
    /// <param name="availableBounds">Bounds supplied by the layout parent.</param>
    /// <param name="descendantsBounds">
    /// Combined final bounds of arranged descendants, or <c>null</c> when there are none.
    /// </param>
    protected virtual GuiBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
        => LayoutParameters.ResolveBounds(
            availableBounds,
            descendantsBounds);
}
