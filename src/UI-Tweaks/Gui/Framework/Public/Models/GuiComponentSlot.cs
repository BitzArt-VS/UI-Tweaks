namespace BitzArt.UI.Tweaks.Gui;

public sealed class GuiComponentSlot : GuiSlot
{
    private readonly IGuiComponent _component;

    /// <summary>
    /// Current arrangement bounds for the mounted component.
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
    public GuiBounds? Bounds { get; set; }

    /// <summary>
    /// Current inner bounds available to descendants after applying padding.
    /// </summary>
    /// <remarks>
    /// This value is provisional while the slot is arranging and final afterward.
    /// </remarks>
    public GuiBounds? ContentBounds { get; set; }

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
        GuiBounds availableBounds)
    {
        Bounds = _component.Arrange(availableBounds);
        ContentBounds =
            Bounds - _component.LayoutParameters.Padding;

        return Bounds.Value;
    }
}
