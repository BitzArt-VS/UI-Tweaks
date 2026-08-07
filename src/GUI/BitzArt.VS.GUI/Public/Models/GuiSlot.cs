using Vintagestory.API.Client;

namespace BitzArt.VS.GUI;

/// <summary>
/// Represents a node's stable mounted position and provides its runtime services.
/// </summary>
public abstract class GuiSlot
{
    private readonly GuiSurfaceRenderer _renderer;

    internal readonly IGuiNode Instance;
    internal readonly GuiTreeBuilder ChildTreeBuilder;

    // The frame is stored here so AddComponent<T> can retrieve and reset it on subsequent
    // rebuilds rather than allocating a new instance. Safe to cast back to TreeFrame<T>
    // since the slot key includes the type — the frame type always matches.
    internal readonly GuiTreeBuilder.TreeFrame Frame;

    /// <summary>
    /// Gets or sets this slot's current complete arrangement bounds.
    /// </summary>
    /// <remarks>
    /// While <see cref="IsArranging"/> is <see langword="true"/>, this value is
    /// provisional and may change as arrangement dependencies are resolved. Otherwise,
    /// it is the cached result of the most recently completed arrangement operation.
    /// <see langword="null"/> means that no arrangement result is currently available.
    /// Reading this property does not initiate arrangement.
    /// </remarks>
    public GuiComponentBounds? Bounds { get; set; }

    public bool IsArranging { get; private set; }

    internal GuiCallback<GuiMouseEventArgs>? OnMouseDown;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseUp;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseClick;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseMove;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseEnter;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseLeave;
    internal GuiCallback<GuiMouseEventArgs>? OnMouseWheel;
    internal IGuiNode? MouseDownFocusTarget;
    internal Predicate<GuiMouseEventArgs>? MouseDownFocusCondition;

    internal GuiCallback<GuiKeyEventArgs>? OnKeyDown;
    internal GuiCallback<GuiKeyEventArgs>? OnKeyUp;
    internal GuiCallback<GuiKeyEventArgs>? OnKeyPress;
    internal GuiCallback<bool>? OnFocusChanged;

    internal GuiSlot(
        GuiSurfaceRenderer renderer,
        GuiSlot? parent,
        IGuiNode instance,
        GuiTreeBuilder childTreeBuilder,
        GuiTreeBuilder.TreeFrame frame)
    {
        _renderer = renderer;
        Parent = parent;
        Instance = instance;
        ChildTreeBuilder = childTreeBuilder;
        Frame = frame;
    }

    public ICoreClientAPI ClientApi => _renderer.ClientApi;

    /// <summary>
    /// Gets the immediate structural parent, or <c>null</c> for a root slot.
    /// </summary>
    public GuiSlot? Parent { get; }

    public IGuiNode Node => Instance;
    public IReadOnlyList<GuiSlot> Children => ChildTreeBuilder.NodeSlots;

    internal bool HasMouseHandlers =>
        OnMouseDown is not null || OnMouseUp is not null
        || OnMouseClick is not null || OnMouseMove is not null
        || OnMouseEnter is not null || OnMouseLeave is not null
        || OnMouseWheel is not null || MouseDownFocusTarget is not null;

    internal bool HasKeyboardRegionHandlers =>
        OnKeyDown is not null || OnKeyUp is not null || OnKeyPress is not null || OnFocusChanged is not null;

    public void RequestReconcile()
        => _renderer.Schedule(Instance.TreeFragment, ChildTreeBuilder);

    public void RequestArrange()
        => _renderer.RequestArrange();

    public void RequestRender()
        => _renderer.RequestRender();

    public bool TryGetCascadingValue<T>(out T value)
        => TryGetCascadingValue(name: null, out value);

    public bool TryGetCascadingValue<T>(string? name, out T value)
    {
        var chain = ChildTreeBuilder.InheritedCascadeChain;
        if (chain is null)
        {
            value = default!;
            return false;
        }

        return chain.TryGet(name, out value);
    }

    /// <summary>
    /// Arranges this root slot within resolved available bounds.
    /// </summary>
    /// <returns>
    /// Resolved arranged bounds, or <see langword="null"/> when the slot has no relative
    /// layout extent.
    /// </returns>
    public GuiBounds? Arrange(GuiBounds availableBounds)
    {
        var arrangedBounds =
            Arrange(
                availableBounds,
                availableBounds);

        if (arrangedBounds is null)
        {
            return null;
        }

        return new GuiBounds(
            arrangedBounds.Value.Position?.Resolve(
                availableBounds.Position
                ?? throw new InvalidOperationException(
                    "Root arrangement requires a resolved parent position.")),
            arrangedBounds.Value.Size);
    }

    internal GuiBounds? Arrange(
        GuiBounds availableBounds,
        GuiBounds completeBounds)
    {
        if (IsArranging)
        {
            throw new InvalidOperationException(
                $"Arrangement cycle detected at {Instance.GetType().Name}.");
        }

        IsArranging = true;
        try
        {
            return OnArrange(
                availableBounds,
                completeBounds);
        }
        finally
        {
            IsArranging = false;
        }
    }

    private protected abstract GuiBounds? OnArrange(
        GuiBounds availableBounds,
        GuiBounds completeBounds);

    /// <summary>
    /// Arranges each immediate child slot in declaration order.
    /// Transparent child slots recursively forward arrangement to their own children.
    /// </summary>
    public GuiBounds? ArrangeChildren(GuiBounds availableBounds)
        => throw new NotImplementedException();

}
