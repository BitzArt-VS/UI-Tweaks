namespace BitzArt.VS.GUI;

public sealed class GuiComponentSlot : GuiSlot
{
    private readonly IGuiComponent _component;

    /// <summary>
    /// Current complete arrangement bounds for the mounted component.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While <see cref="GuiSlot.IsArranging"/> is <see langword="true"/>, this value is
    /// provisional and may change as arrangement dependencies are resolved. Otherwise,
    /// it is the cached result of the most recently completed arrangement operation.
    /// </para>
    /// <para>
    /// <see langword="null"/> means that no arrangement result is currently available.
    /// Reading this property does not initiate arrangement.
    /// </para>
    /// </remarks>
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
