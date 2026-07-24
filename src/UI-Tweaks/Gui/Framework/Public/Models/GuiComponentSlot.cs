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

    private protected override void OnArrange()
        => Bounds = _component.Arrange();

    internal void SetBounds(GuiComponentBounds bounds)
    {
        IsScrollable = false;
        Bounds = bounds;
        ScrollClipBounds = default;
    }
}
