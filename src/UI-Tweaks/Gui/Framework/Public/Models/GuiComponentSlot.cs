namespace BitzArt.UI.Tweaks.Gui;

public sealed class GuiComponentSlot : GuiSlot
{
    private readonly IGuiComponent _component;

    public GuiComponentBounds? Bounds { get; set; }

    internal GuiComponentSlot(
        GuiSurfaceRenderer renderer,
        GuiSlot? parent,
        IGuiComponent component,
        GuiTreeBuilder childTreeBuilder,
        GuiTreeBuilder.TreeFrame frame)
        : base(renderer, parent, component, childTreeBuilder, frame)
    {
        _component = component;
    }

    internal IGuiComponent Component => _component;

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
            Bounds = _component.Arrange();
        }
        finally
        {
            IsArranging = false;
        }
    }

    internal void SetBounds(GuiComponentBounds bounds)
    {
        IsScrollable = false;
        Bounds = bounds;
        ScrollClipBounds = default;
    }
}
