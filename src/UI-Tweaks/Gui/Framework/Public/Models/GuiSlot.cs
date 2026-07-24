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
    public bool IsArranging { get; private protected set; }
    internal GuiComponentBounds ScrollClipBounds;

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

    public abstract void Arrange();

    internal void SetScrollableBounds(GuiComponentBounds scrollClipBounds)
    {
        IsScrollable = true;
        ScrollClipBounds = scrollClipBounds;
    }
}
