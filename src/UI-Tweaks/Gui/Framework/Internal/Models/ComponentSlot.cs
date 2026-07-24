using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

internal sealed class ComponentSlot(
    GuiSurfaceRenderer renderer,
    ComponentSlot? parent,
    IGuiNode instance,
    GuiTreeBuilder childTreeBuilder,
    GuiTreeBuilder.TreeFrame frame)
    : IGuiNodeSlot
{
    private readonly GuiSurfaceRenderer _renderer = renderer;

    public readonly IGuiNode Instance = instance;
    public readonly GuiTreeBuilder ChildTreeBuilder = childTreeBuilder;

    // The frame is stored here so AddComponent<T> can retrieve and reset it on subsequent
    // rebuilds rather than allocating a new instance. Safe to cast back to TreeFrame<T>
    // since the slot key includes the type — the frame type always matches.
    public readonly GuiTreeBuilder.TreeFrame Frame = frame;

    public bool IsScrollable;
    public GuiComponentBounds? Bounds { get; private set; }
    public GuiComponentBounds ScrollClipBounds;

    public GuiCallback<GuiMouseEventArgs> OnMouseDown;
    public GuiCallback<GuiMouseEventArgs> OnMouseUp;
    public GuiCallback<GuiMouseEventArgs> OnMouseClick;
    public GuiCallback<GuiMouseEventArgs> OnMouseMove;
    public GuiCallback<GuiMouseEventArgs> OnMouseEnter;
    public GuiCallback<GuiMouseEventArgs> OnMouseLeave;

    public GuiCallback<GuiKeyEventArgs> OnKeyDown;
    public GuiCallback<GuiKeyEventArgs> OnKeyUp;
    public GuiCallback<GuiKeyEventArgs> OnKeyPress;
    public GuiCallback<bool> OnFocusChanged;

    public ICoreClientAPI ClientApi => _renderer.ClientApi;
    public IGuiNodeSlot? Parent { get; } = parent;
    public IGuiNode Node => Instance;
    public IReadOnlyList<IGuiNodeSlot> Children => ChildTreeBuilder.NodeSlots;

    public bool HasMouseHandlers =>
        OnMouseDown.HasHandler || OnMouseUp.HasHandler
        || OnMouseClick.HasHandler || OnMouseMove.HasHandler
        || OnMouseEnter.HasHandler || OnMouseLeave.HasHandler;

    public bool HasKeyboardRegionHandlers =>
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

    public GuiComponentBounds Arrange(bool layoutChanged = false)
    {
        if (Instance is not IGuiComponent component)
        {
            throw new InvalidOperationException(
                "A layout-transparent node cannot be arranged as a component.");
        }

        if (!layoutChanged && Bounds is GuiComponentBounds arrangedBounds)
        {
            return arrangedBounds;
        }

        GuiComponentBounds bounds = component.Arrange();
        Bounds = bounds;

        return bounds;
    }

    public void SetLayoutTransparentBounds(GuiComponentBounds bounds)
    {
        IsScrollable = false;
        Bounds = bounds;
    }

    public void SetComponentBounds(GuiComponentBounds bounds)
    {
        IsScrollable = false;
        Bounds = bounds;
        ScrollClipBounds = default;
    }

    public void SetScrollableBounds(GuiComponentBounds scrollClipBounds)
    {
        IsScrollable = true;
        ScrollClipBounds = scrollClipBounds;
    }
}
