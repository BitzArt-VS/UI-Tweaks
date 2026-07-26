using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

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

    internal bool IsScrollable;
    public bool IsArranging { get; private set; }
    internal GuiBounds ScrollClipBounds;

    internal GuiCallback<GuiMouseEventArgs> OnMouseDown;
    internal GuiCallback<GuiMouseEventArgs> OnMouseUp;
    internal GuiCallback<GuiMouseEventArgs> OnMouseClick;
    internal GuiCallback<GuiMouseEventArgs> OnMouseMove;
    internal GuiCallback<GuiMouseEventArgs> OnMouseEnter;
    internal GuiCallback<GuiMouseEventArgs> OnMouseLeave;

    internal GuiCallback<GuiKeyEventArgs> OnKeyDown;
    internal GuiCallback<GuiKeyEventArgs> OnKeyUp;
    internal GuiCallback<GuiKeyEventArgs> OnKeyPress;
    internal GuiCallback<bool> OnFocusChanged;

    internal GuiSlot(
        GuiSurfaceRenderer renderer,
        GuiSlot? parent,
        IGuiNode instance,
        GuiTreeBuilder childTreeBuilder,
        GuiTreeBuilder.TreeFrame frame)
    {
        _renderer = renderer;
        Parent = parent;
        LayoutParent = parent as GuiComponentSlot
            ?? parent?.LayoutParent;
        Instance = instance;
        ChildTreeBuilder = childTreeBuilder;
        Frame = frame;
    }

    public ICoreClientAPI ClientApi => _renderer.ClientApi;

    /// <summary>
    /// Gets the immediate structural parent, or <c>null</c> for a root slot.
    /// </summary>
    public GuiSlot? Parent { get; }

    /// <summary>
    /// Gets the nearest ancestor whose node participates in layout, or <c>null</c>
    /// when the slot has no component ancestor.
    /// </summary>
    public GuiComponentSlot? LayoutParent { get; }

    public IGuiNode Node => Instance;
    public IReadOnlyList<GuiSlot> Children => ChildTreeBuilder.NodeSlots;

    internal bool HasMouseHandlers =>
        OnMouseDown.HasHandler || OnMouseUp.HasHandler
        || OnMouseClick.HasHandler || OnMouseMove.HasHandler
        || OnMouseEnter.HasHandler || OnMouseLeave.HasHandler;

    internal bool HasKeyboardRegionHandlers =>
        OnKeyDown.HasHandler || OnKeyUp.HasHandler || OnKeyPress.HasHandler || OnFocusChanged.HasHandler;

    public void RequestReconcile()
        => _renderer.Schedule(Instance.TreeFragment, ChildTreeBuilder);

    public void RequestLayout()
        => _renderer.RequestLayout();

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

    public GuiBounds? Arrange(GuiBounds availableBounds)
    {
        if (IsArranging)
        {
            throw new InvalidOperationException(
                $"Arrangement cycle detected at {Instance.GetType().Name}.");
        }

        IsArranging = true;
        try
        {
            return OnArrange(availableBounds);
        }
        finally
        {
            IsArranging = false;
        }
    }

    private protected abstract GuiBounds? OnArrange(
        GuiBounds availableBounds);

    /// <summary>
    /// Arranges each immediate child slot in declaration order.
    /// Transparent child slots recursively forward arrangement to their own children.
    /// </summary>
    public GuiBounds? ArrangeChildren(
        GuiBounds availableBounds)
    {
        GuiBounds? extent = null;

        foreach (GuiSlot child in Children)
        {
            GuiBounds? childBounds =
                child.Arrange(availableBounds);

            if (childBounds is GuiBounds bounds)
            {
                extent = extent is null
                    ? bounds
                    : Union(extent.Value, bounds);
            }
        }

        return extent;
    }

    private static GuiBounds Union(
        GuiBounds first,
        GuiBounds second)
    {
        if (first.Position is not GuiPoint firstPosition
            || second.Position is not GuiPoint secondPosition)
        {
            return new GuiBounds(null, null);
        }

        double left = Math.Min(firstPosition.X, secondPosition.X);
        double top = Math.Min(firstPosition.Y, secondPosition.Y);

        double? right =
            GetEnd(firstPosition.X, first.Size?.Width)
            is double firstRight
            && GetEnd(secondPosition.X, second.Size?.Width)
            is double secondRight
                ? Math.Max(firstRight, secondRight)
                : null;

        double? bottom =
            GetEnd(firstPosition.Y, first.Size?.Height)
            is double firstBottom
            && GetEnd(secondPosition.Y, second.Size?.Height)
            is double secondBottom
                ? Math.Max(firstBottom, secondBottom)
                : null;

        return new GuiBounds(
            new GuiPoint(left, top),
            new GuiSize(
                right - left,
                bottom - top));
    }

    private static double? GetEnd(
        double start,
        double? length)
        => length is double resolvedLength
            ? start + resolvedLength
            : null;

    internal void SetScrollableBounds(GuiBounds scrollClipBounds)
    {
        IsScrollable = true;
        ScrollClipBounds = scrollClipBounds;
    }
}
