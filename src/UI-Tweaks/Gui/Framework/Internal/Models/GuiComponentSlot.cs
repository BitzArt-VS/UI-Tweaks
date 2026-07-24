namespace BitzArt.UI.Tweaks.Gui;

internal sealed class GuiComponentSlot(
    GuiSurfaceRenderer renderer,
    GuiSlot? parent,
    IGuiComponent component,
    GuiTreeBuilder childTreeBuilder,
    GuiTreeBuilder.TreeFrame frame)
    : GuiSlot(renderer, parent, component, childTreeBuilder, frame)
{
    private readonly IGuiComponent _component = component;
    private bool _isArranging;

    public override bool IsArranging => _isArranging;

    public override void Arrange()
    {
        if (IsArranging)
        {
            throw new InvalidOperationException(
                $"Arrangement cycle detected at {Instance.GetType().Name}.");
        }

        _isArranging = true;
        try
        {
            Bounds = _component.Arrange();
        }
        finally
        {
            _isArranging = false;
        }
    }
}
