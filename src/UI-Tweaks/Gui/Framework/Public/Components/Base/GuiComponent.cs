namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Default base class for layout-participating components. Extends <see cref="GuiNode"/>
/// with the <see cref="LayoutParameters"/> bundle and a virtual <see cref="Measure"/>
/// hook consumed by the layout pass. The default measurement walks the component's
/// mounted child slots and applies the framework's stack-layout sizing rules. Pure
/// decorators that do not occupy layout space should inherit from <see cref="GuiNode"/>
/// directly instead.
/// </summary>
public abstract class GuiComponent : GuiNode, IGuiComponent
{
    public GuiComponentLayoutParameters LayoutParameters { get; }

    protected GuiComponent()
    {
        LayoutParameters = new GuiComponentLayoutParameters();
    }

    /// <inheritdoc/>
    public virtual GuiLayoutSize Measure(GuiLayoutSize available)
    {
        if (Slot is null)
        {
            return default;
        }

        return GuiComponentLayout.MeasureContent(
            Slot.Children,
            available,
            LayoutParameters.Direction);
    }

    /// <summary>
    /// Requests a fresh layout pass for the existing component tree. Layout cascades into rendering.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown if the component is not attached to a slot.</exception>
    protected void RequestLayout()
    {
        GetAttachedSlot(nameof(RequestLayout)).RequestLayout();
    }

    /// <summary>
    /// Requests rendering of the existing component tree without scheduling this component's
    /// render fragment for reconciliation.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown if the component is not attached to a slot.</exception>
    protected void RequestRender()
    {
        GetAttachedSlot(nameof(RequestRender)).RequestRender();
    }

}
