namespace BitzArt.UI.Tweaks.Gui;

public interface IGuiComponent : IGuiNode
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    /// <summary>
    /// Returns the component's desired content size given the available space.
    /// </summary>
    public GuiLayoutSize Measure(GuiLayoutSize available);
}
