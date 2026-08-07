namespace BitzArt.VS.GUI;

public sealed class GuiComponentSlot : GuiSlot
{
    private readonly IGuiComponent _component;

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

    private protected override GuiBounds? OnArrange(
        GuiBounds availableBounds,
        GuiBounds completeBounds)
    {
        GuiComponentBounds arrangedBounds =
            _component.Arrange(availableBounds);

        if (arrangedBounds.Bounds.Position?.IsAbsolute == true
            && availableBounds != completeBounds)
        {
            arrangedBounds =
                _component.Arrange(completeBounds);
        }

        ResolveBounds(arrangedBounds, availableBounds);

        return arrangedBounds.MarginBounds;
    }

    internal void ResolveBounds(
        GuiComponentBounds componentBounds,
        GuiBounds parentBounds)
    {
        var parentPosition =
            parentBounds.Position
            ?? throw new InvalidOperationException(
                "Component bounds require a resolved parent position.");

        Bounds = componentBounds with
        {
            Bounds = componentBounds.Bounds with
            {
                Position = componentBounds.Bounds.Position?.Resolve(parentPosition),
            },
        };
    }
}
