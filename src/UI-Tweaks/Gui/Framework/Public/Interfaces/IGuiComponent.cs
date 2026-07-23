namespace BitzArt.UI.Tweaks.Gui;

public interface IGuiComponent : IGuiNode
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    /// <summary>
    /// Attempts to arrange this component using layout context available through
    /// its mounted slot.
    /// </summary>
    /// <returns>
    /// <c>null</c> when the component's arranged size cannot yet be resolved;
    /// otherwise, the resolved size and an optional resolved position.
    /// </returns>
    public GuiComponentBounds? Arrange();
}
