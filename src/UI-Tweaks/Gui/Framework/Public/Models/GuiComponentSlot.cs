namespace BitzArt.UI.Tweaks.Gui;

public sealed class GuiComponentSlot : GuiSlot
{
    private readonly IGuiComponent _component;
    private bool _isArranging;

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

    public GuiComponentBounds? Bounds { get; set; }

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

    internal void SetBounds(GuiComponentBounds bounds)
    {
        IsScrollable = false;
        Bounds = bounds;
        ScrollClipBounds = default;
    }
}
