namespace BitzArt.UI.Tweaks.Gui;

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
    public GuiBounds Arrange(
        GuiBounds availableBounds);
}
