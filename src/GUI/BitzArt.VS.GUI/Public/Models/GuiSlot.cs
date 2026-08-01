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
        => ArrangeChildren(
            availableBounds,
            availableBounds);

    internal GuiBounds? ArrangeChildren(
        GuiBounds availableBounds,
        GuiBounds completeBounds)
    {
        var remainingVerticalBounds = availableBounds;
        var remainingBounds = availableBounds;
        GuiBounds? lineBounds = null;
        GuiBounds? arrangedChildrenBounds = null;

        foreach (var child in Children)
        {
            var childBounds = child.Arrange(
                remainingBounds,
                completeBounds);
            if (childBounds is null)
            {
                continue;
            }

            if (childBounds.Value.Position?.IsAbsolute == true)
            {
                continue;
            }

            var resolvedChildBounds = new GuiBounds(
                childBounds.Value.Position?.Resolve(
                    remainingBounds.Position
                    ?? throw new InvalidOperationException(
                        "A child requires a resolved parent position.")),
                childBounds.Value.Size);

            var marginBounds =
                resolvedChildBounds;

            if (lineBounds is not null
                && ExceedsAvailableWidth(marginBounds, remainingBounds))
            {
                remainingVerticalBounds =
                    remainingVerticalBounds.SubtractVertical(lineBounds.Value);

                remainingBounds = remainingVerticalBounds;
                lineBounds = null;

                childBounds = child.Arrange(
                    remainingBounds,
                    completeBounds);
                if (childBounds is null)
                {
                    continue;
                }

                if (childBounds.Value.Position?.IsAbsolute == true)
                {
                    continue;
                }

                resolvedChildBounds = new GuiBounds(
                    childBounds.Value.Position?.Resolve(
                        remainingBounds.Position
                        ?? throw new InvalidOperationException(
                            "A child requires a resolved parent position.")),
                    childBounds.Value.Size);

                marginBounds =
                    resolvedChildBounds;
            }

            arrangedChildrenBounds = arrangedChildrenBounds is null
                ? marginBounds
                : arrangedChildrenBounds.Value.Union(marginBounds);

            lineBounds = lineBounds is null
                ? marginBounds
                : lineBounds.Value.Union(marginBounds);

            remainingBounds =
                remainingBounds.SubtractHorizontal(marginBounds);

            if (remainingBounds.Size?.Width == 0)
            {
                remainingVerticalBounds =
                    remainingVerticalBounds.SubtractVertical(lineBounds.Value);

                remainingBounds = remainingVerticalBounds;
                lineBounds = null;
            }
        }

        if (arrangedChildrenBounds is null)
        {
            return null;
        }

        return new GuiBounds(
            arrangedChildrenBounds.Value.Position is GuiPoint arrangedPosition
                ? new GuiPoint(
                    arrangedPosition.X
                        - (availableBounds.Position
                            ?? throw new InvalidOperationException(
                                "Arranged children require an available origin."))
                            .X,
                    arrangedPosition.Y - availableBounds.Position.Value.Y)
                : null,
            arrangedChildrenBounds.Value.Size);
    }

    private static bool ExceedsAvailableWidth(
        GuiBounds contentBounds,
        GuiBounds availableBounds)
    {
        if (contentBounds.Position is not GuiPoint contentPosition
            || contentBounds.Size?.Width is not double contentWidth
            || availableBounds.Position is not GuiPoint availablePosition
            || availableBounds.Size?.Width is not double availableWidth)
        {
            return false;
        }

        return contentPosition.X + contentWidth
            > availablePosition.X + availableWidth;
    }

}
