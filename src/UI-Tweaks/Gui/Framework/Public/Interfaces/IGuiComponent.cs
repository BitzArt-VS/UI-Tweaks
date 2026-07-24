namespace BitzArt.UI.Tweaks.Gui;

public interface IGuiComponent : IGuiNode
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    /// <summary>
    /// Attempts to arrange this component using layout context available through
    /// its mounted slot.
    /// </summary>
    /// <returns>
    /// The arranged bounds. Individual bounds fields are <c>null</c> while
    /// their geometry is unresolved.
    /// </returns>
    public GuiComponentBounds Arrange();
}
