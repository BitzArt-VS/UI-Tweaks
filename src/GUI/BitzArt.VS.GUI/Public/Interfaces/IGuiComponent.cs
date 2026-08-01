namespace BitzArt.VS.GUI;

public interface IGuiComponent : IGuiNode
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    /// <summary>
    /// Attempts to arrange this component within the supplied available bounds.
    /// </summary>
    /// <returns>
    /// Arranged bounds relative to <paramref name="availableBounds"/> for flow placement,
    /// or absolute bounds for independent placement. Individual bounds fields are
    /// <c>null</c> while their geometry is unresolved.
    /// </returns>
    public GuiComponentBounds Arrange(
        GuiBounds availableBounds);
}
