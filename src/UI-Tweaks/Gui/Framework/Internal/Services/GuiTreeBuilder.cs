using Cairo;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Executes <see cref="GuiTreeFragment"/>s, reconciles the resulting
/// <see cref="TreeFrame"/> instructions against previously-known state,
/// and manages the lifetimes of child <see cref="IGuiNode"/> instances.
/// </summary>
internal sealed class GuiTreeBuilder : IGuiTreeBuilder, IDisposable
{
    private readonly GuiSurfaceRenderer _renderer;
    private GuiSlot? _ownerSlot;

    // Frame buffer: filled during the blueprint phase, cleared at the start of each Run().
    private readonly List<TreeFrame> _frames = [];

    // Persistent storage for keyed slots. Key is a value-type struct — no frame allocation
    // needed just for identity.
    private readonly Dictionary<ComponentSlotKey, GuiSlot> _keyedSlots = [];

    // Ordered list of active slots, rebuilt each Run() to match the current frame order.
    // Used by arrange and paint walks to iterate children in declaration order.
    private readonly List<GuiSlot> _renderOrder = [];
    private readonly IReadOnlyList<GuiSlot> _nodeSlots;

    // Reused scratch buffers — avoid allocating inside the hot path.
    private readonly HashSet<ComponentSlotKey> _seenKeys = [];
    private readonly List<ComponentSlotKey> _staleKeys = [];

    // Cascading-value chain visible to the component that owns this builder. Parent
    // reconciliation sets this to the chain snapshotted at the owning slot's declaration site.
    internal CascadingValueChain? InheritedCascadeChain;

    // Cascading-value chain visible to slots declared inside this builder. Parent
    // reconciliation initializes it from InheritedCascadeChain; PushCascadeScope mutates it
    // temporarily during this builder's blueprint phase for descendants declared inside the scope.
    internal CascadingValueChain? CascadeChain;

    internal IReadOnlyList<GuiSlot> NodeSlots => _nodeSlots;

    internal GuiTreeBuilder(GuiSurfaceRenderer renderer)
    {
        _renderer = renderer;
        _nodeSlots = _renderOrder.AsReadOnly();
    }

    public IGuiTreeBuilder<T> AddComponent<T>(int key)
        where T : IGuiNode, new()
    {
        var slotKey = new ComponentSlotKey(typeof(T), key);

        // Detect duplicate (Type, key) declarations within the current blueprint phase.
        // _seenKeys is cleared at the start of every Run() and populated here, so a key
        // that's already present means the same slot was declared twice among siblings.
        if (!_seenKeys.Add(slotKey))
        {
            throw new InvalidOperationException(
                $"Duplicate component key {key} for {typeof(T).Name} within the same render tree level. Each (Type, key) pair must be unique among siblings.");
        }

        TreeFrame<T> frame;

        if (_keyedSlots.TryGetValue(slotKey, out var existingSlot))
        {
            // Reuse the frame that lives inside the persistent slot — zero allocation in steady state.
            frame = (TreeFrame<T>)existingSlot.Frame;
            // Discard any actions accumulated on the previous pass. Actions are re-registered
            // each pass by the user's BuildComponentTree, so per-pass values configured
            // through Configure and ConfigureLayout take effect on the next pass.
            frame.Reset();
        }
        else
        {
            frame = new TreeFrame<T>(this, key);
        }

        _frames.Add(frame);
        // Snapshot the chain that is active right now in the blueprint phase. A surrounding
        // PushCascadeScope call may have already pushed a link onto CascadeChain, so this
        // records the exact scope visible at this slot's declaration site. The patch phase
        // reads frame.CascadeChain to set the child builder's chain, ensuring descendants
        // see the correct set of cascading values regardless of push/pop interleaving.
        frame.CascadeChain = CascadeChain;
        return frame;
    }

    /// <summary>
    /// Pushes a cascading value scope for the duration of <paramref name="content"/>.
    /// All slots declared inside <paramref name="content"/> at any depth will see
    /// <paramref name="value"/> (matched by <c>typeof(<typeparamref name="T"/>)</c> and
    /// <paramref name="name"/>) when they query the cascade chain. The scope is stack-based
    /// and is restored after <paramref name="content"/> returns — no component is created,
    /// no slot is allocated, and the layout tree is completely unaffected.
    /// </summary>
    public void PushCascadeScope<T>(T value, string? name, GuiTreeFragment content)
    {
        var saved = CascadeChain;
        CascadeChain = new CascadingValueChain(saved, typeof(T), name, value);
        content.Invoke(this);
        CascadeChain = saved;
    }

    /// <summary>
    /// Runs one reconciliation pass.
    /// <list type="number">
    ///   <item><description><b>Blueprint phase</b> — executes <paramref name="fragment"/>, filling the frame buffer.</description></item>
    ///   <item><description><b>Diff phase</b> — compares frames against existing keyed slots.</description></item>
    ///   <item><description><b>Patch phase</b> — pushes configure into reused instances; creates new ones where needed; prunes stale keyed slots; recurses into each component's own children.</description></item>
    /// </list>
    /// </summary>
    internal void Run(GuiTreeFragment fragment)
    {
        _frames.Clear();
        _renderOrder.Clear();
        // Cleared up-front so AddComponent can populate it during the blueprint phase
        // for both duplicate-key detection and stale-pruning below.
        _seenKeys.Clear();
        fragment.Invoke(this);

        foreach (var frame in _frames)
        {
            var slotKey = new ComponentSlotKey(frame.ComponentType, frame.Key);

            bool isNew = !_keyedSlots.TryGetValue(slotKey, out var slot);
            if (isNew)
            {
                slot = CreateSlot(frame);
                _keyedSlots[slotKey] = slot;
            }
            ReconcileSlot(slot!, frame, isNew);
            _renderOrder.Add(slot!);
        }

        _staleKeys.Clear();
        foreach (var key in _keyedSlots.Keys)
        {
            if (!_seenKeys.Contains(key))
            {
                _staleKeys.Add(key);
            }
        }
        foreach (var key in _staleKeys)
        {
            DisposeSlot(_keyedSlots[key]);
            _keyedSlots.Remove(key);
        }
    }

    /// <summary>
    /// Disposes every slot owned by this builder, asking each slot's child builder to do
    /// the same for its own subtree first. Called when a parent slot is pruned (so its
    /// child builder must release everything it owns) and when the dialog is being torn down.
    /// </summary>
    public void Dispose()
    {
        foreach (var slot in _keyedSlots.Values)
        {
            DisposeSlot(slot);
        }

        _keyedSlots.Clear();
        _renderOrder.Clear();
        _frames.Clear();
    }

    private static void DisposeSlot(GuiSlot slot)
    {
        // Children first — the parent instance may rely on its subtree still existing
        // during its own Dispose (e.g. unsubscribing from child events).
        slot.ChildTreeBuilder.Dispose();
        (slot.Instance as IDisposable)?.Dispose();
    }


    private GuiSlot CreateSlot(TreeFrame frame)
    {
        var childBuilder = new GuiTreeBuilder(_renderer);
        var instance = frame.CreateInstance();
        GuiSlot slot = instance switch
        {
            IGuiComponent component => new GuiComponentSlot(
                _renderer,
                _ownerSlot,
                component,
                childBuilder,
                frame),
            _ => new GuiNodeSlot(
                _renderer,
                _ownerSlot,
                instance,
                childBuilder,
                frame),
        };

        childBuilder._ownerSlot = slot;
        instance.Attach(slot);
        return slot;
    }

    private void ReconcileSlot(GuiSlot slot, TreeFrame frame, bool isNew)
    {
        // Propagate the declaration-site cascade chain before any component callbacks run.
        // Configure/OnInitialized/OnParametersSet may all read cascading values from the
        // render handle, and descendants declared by this slot consume the same chain as
        // their inherited parent scope during their own blueprint phase.
        slot.ChildTreeBuilder.InheritedCascadeChain = frame.CascadeChain;
        slot.ChildTreeBuilder.CascadeChain = frame.CascadeChain;

        if (slot.Instance is IGuiComponent component)
        {
            // Reset layout parameters to canonical defaults before applying the new
            // pass's config actions — blueprints are declarative (full state), not
            // deltas. Without this, stale LP from a previous view (e.g. a list column
            // that becomes a setting row at the same key) would persist across reuses.
            component.LayoutParameters.Reset();
        }

        frame.ApplySlotConfiguration(slot.Instance);
        frame.ApplyConfiguration(slot.Instance);
        frame.ComposeSlotConfiguration(slot);
        if (isNew)
        {
            slot.Instance.OnInitialized();
        }
        slot.Instance.OnParametersSet();
        // Cancel any separately scheduled rebuild for this child's fragment — we are
        // about to rebuild its subtree right now, making the pending entry redundant.
        _renderer.Cancel(slot.Instance.TreeFragment);
        slot.ChildTreeBuilder.Run(slot.Instance.TreeFragment);
    }

    internal void ArrangeRoot(
        GuiBounds availableBounds)
    {
        if (_ownerSlot is not null)
        {
            throw new InvalidOperationException(
                "Only a root tree builder can arrange the GUI root.");
        }

        if (_renderOrder.Count != 1)
        {
            throw new InvalidOperationException(
                "A GUI tree must contain exactly one root node.");
        }

        _renderOrder[0].Arrange(availableBounds);
    }

    internal void Paint(Context context, bool registerRegions)
        => PaintInto(context, registerRegions, inheritedClipBounds: null);

    private GuiBounds? PaintInto(Context context, bool registerRegions, GuiBounds? inheritedClipBounds)
    {
        GuiBounds? extent = null;

        foreach (GuiSlot slot in _renderOrder)
        {
            if (slot is not GuiComponentSlot componentSlot)
            {
                GuiBounds? childExtent =
                    PaintDescendants(
                        slot,
                        context,
                        registerRegions,
                        inheritedClipBounds);

                if (childExtent is not GuiBounds wrapperBounds)
                {
                    continue;
                }

                slot.Instance.Render(context, wrapperBounds);

                if (registerRegions)
                {
                    RegisterRegions(slot, wrapperBounds, inheritedClipBounds);
                }

                slot.Instance.RenderOverlay(context, wrapperBounds);
                extent = Union(extent, wrapperBounds);
                continue;
            }

            if (componentSlot.Bounds is not GuiBounds bounds)
            {
                continue;
            }

            IGuiComponent layoutComponent = componentSlot.Component;
            extent = Union(extent, bounds);
            layoutComponent.Render(context, bounds);

            if (registerRegions)
            {
                RegisterRegions(slot, bounds, inheritedClipBounds);
            }

            PaintDescendants(
                slot,
                context,
                registerRegions,
                inheritedClipBounds);
            layoutComponent.RenderOverlay(context, bounds);
        }

        return extent;
    }

    private GuiBounds? PaintDescendants(
        GuiSlot slot,
        Context context,
        bool registerRegions,
        GuiBounds? inheritedClipBounds)
    {
        if (!_renderer.ClippingContext.TryGetClip(
                slot.Instance,
                out var descendantClipBounds))
        {
            return slot.ChildTreeBuilder.PaintInto(
                context,
                registerRegions,
                inheritedClipBounds);
        }

        var clipPosition = descendantClipBounds.Position!.Value;
        var clipSize = descendantClipBounds.Size!.Value;

        context.Save();
        try
        {
            context.Rectangle(
                clipPosition.X,
                clipPosition.Y,
                clipSize.Width!.Value,
                clipSize.Height!.Value);
            context.Clip();

            return slot.ChildTreeBuilder.PaintInto(
                context,
                registerRegions,
                IntersectClipBounds(
                    inheritedClipBounds,
                    descendantClipBounds));
        }
        finally
        {
            context.Restore();
        }
    }

    private void RegisterRegions(GuiSlot slot, GuiBounds bounds, GuiBounds? clipBounds)
    {
        if (slot.Instance is IGuiResizable resizable && resizable.SupportedResizeEdges != GuiResizeEdge.None)
        {
            _renderer.AddResizeRegion(new ResizeRegion(bounds, slot.Instance, resizable, clipBounds));
        }

        if (slot.HasMouseHandlers)
        {
            _renderer.AddInteractiveRegion(new InteractiveRegion(
                bounds,
                slot.Instance,
                slot.OnMouseDown,
                slot.OnMouseUp,
                slot.OnMouseClick,
                slot.OnMouseMove,
                slot.OnMouseEnter,
                slot.OnMouseLeave,
                slot.OnMouseWheel,
                clipBounds: clipBounds));
        }

        if (slot.HasKeyboardRegionHandlers)
        {
            _renderer.AddKeyboardRegion(new KeyboardRegion(
                slot.Instance,
                slot.OnKeyDown,
                slot.OnKeyUp,
                slot.OnKeyPress,
                slot.OnFocusChanged));
        }
    }

    private static GuiBounds? IntersectClipBounds(GuiBounds? first, GuiBounds second)
    {
        if (first is not GuiBounds firstBounds)
        {
            return second;
        }

        var firstPosition = firstBounds.Position!.Value;
        var firstSize = firstBounds.Size!.Value;
        var secondPosition = second.Position!.Value;
        var secondSize = second.Size!.Value;

        var left = Math.Max(firstPosition.X, secondPosition.X);
        var top = Math.Max(firstPosition.Y, secondPosition.Y);
        var right = Math.Min(
            firstPosition.X + firstSize.Width!.Value,
            secondPosition.X + secondSize.Width!.Value);
        var bottom = Math.Min(
            firstPosition.Y + firstSize.Height!.Value,
            secondPosition.Y + secondSize.Height!.Value);

        return new GuiBounds(
            new GuiPoint(left, top, IsAbsolute: true),
            new GuiSize(
                Math.Max(0, right - left),
                Math.Max(0, bottom - top)));
    }

    private static GuiBounds? Union(GuiBounds? extent, GuiBounds bounds)
        => extent is null
            ? bounds
            : extent.Value.Union(bounds);

    private struct SlotCallbacks
    {
        public GuiCallback<GuiMouseEventArgs> OnMouseDown;
        public GuiCallback<GuiMouseEventArgs> OnMouseUp;
        public GuiCallback<GuiMouseEventArgs> OnMouseClick;
        public GuiCallback<GuiMouseEventArgs> OnMouseMove;
        public GuiCallback<GuiMouseEventArgs> OnMouseEnter;
        public GuiCallback<GuiMouseEventArgs> OnMouseLeave;
        public GuiCallback<GuiMouseEventArgs> OnMouseWheel;

        public GuiCallback<GuiKeyEventArgs> OnKeyDown;
        public GuiCallback<GuiKeyEventArgs> OnKeyUp;
        public GuiCallback<GuiKeyEventArgs> OnKeyPress;
        public GuiCallback<bool> OnFocusChanged;

        public void AddMouseHandler(GuiMouseEventKind kind, GuiCallback<GuiMouseEventArgs> callback)
        {
            switch (kind)
            {
                case GuiMouseEventKind.Down:
                    OnMouseDown = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseDown, callback);
                    break;
                case GuiMouseEventKind.Up:
                    OnMouseUp = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseUp, callback);
                    break;
                case GuiMouseEventKind.Click:
                    OnMouseClick = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseClick, callback);
                    break;
                case GuiMouseEventKind.Move:
                    OnMouseMove = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseMove, callback);
                    break;
                case GuiMouseEventKind.Enter:
                    OnMouseEnter = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseEnter, callback);
                    break;
                case GuiMouseEventKind.Leave:
                    OnMouseLeave = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseLeave, callback);
                    break;
                case GuiMouseEventKind.Wheel:
                    OnMouseWheel = GuiCallback<GuiMouseEventArgs>.Combine(OnMouseWheel, callback);
                    break;
            }
        }

        public void AddKeyHandler(GuiKeyEventKind kind, GuiCallback<GuiKeyEventArgs> callback)
        {
            switch (kind)
            {
                case GuiKeyEventKind.Down:
                    OnKeyDown = GuiCallback<GuiKeyEventArgs>.Combine(OnKeyDown, callback);
                    break;
                case GuiKeyEventKind.Up:
                    OnKeyUp = GuiCallback<GuiKeyEventArgs>.Combine(OnKeyUp, callback);
                    break;
                case GuiKeyEventKind.Press:
                    OnKeyPress = GuiCallback<GuiKeyEventArgs>.Combine(OnKeyPress, callback);
                    break;
            }
        }

        public void AddFocusChangedHandler(GuiCallback<bool> callback)
        {
            OnFocusChanged = GuiCallback<bool>.Combine(OnFocusChanged, callback);
        }

        public static SlotCallbacks Combine(SlotCallbacks ownCallbacks, SlotCallbacks externalCallbacks)
        {
            return new SlotCallbacks
            {
                OnMouseDown = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseDown, externalCallbacks.OnMouseDown),
                OnMouseUp = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseUp, externalCallbacks.OnMouseUp),
                OnMouseClick = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseClick, externalCallbacks.OnMouseClick),
                OnMouseMove = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseMove, externalCallbacks.OnMouseMove),
                OnMouseEnter = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseEnter, externalCallbacks.OnMouseEnter),
                OnMouseLeave = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseLeave, externalCallbacks.OnMouseLeave),
                OnMouseWheel = GuiCallback<GuiMouseEventArgs>.Combine(ownCallbacks.OnMouseWheel, externalCallbacks.OnMouseWheel),
                OnKeyDown = GuiCallback<GuiKeyEventArgs>.Combine(ownCallbacks.OnKeyDown, externalCallbacks.OnKeyDown),
                OnKeyUp = GuiCallback<GuiKeyEventArgs>.Combine(ownCallbacks.OnKeyUp, externalCallbacks.OnKeyUp),
                OnKeyPress = GuiCallback<GuiKeyEventArgs>.Combine(ownCallbacks.OnKeyPress, externalCallbacks.OnKeyPress),
                OnFocusChanged = GuiCallback<bool>.Combine(ownCallbacks.OnFocusChanged, externalCallbacks.OnFocusChanged),
            };
        }

        public void ApplyTo(GuiSlot slot)
        {
            slot.OnMouseDown = OnMouseDown;
            slot.OnMouseUp = OnMouseUp;
            slot.OnMouseClick = OnMouseClick;
            slot.OnMouseMove = OnMouseMove;
            slot.OnMouseEnter = OnMouseEnter;
            slot.OnMouseLeave = OnMouseLeave;
            slot.OnMouseWheel = OnMouseWheel;
            slot.OnKeyDown = OnKeyDown;
            slot.OnKeyUp = OnKeyUp;
            slot.OnKeyPress = OnKeyPress;
            slot.OnFocusChanged = OnFocusChanged;
        }
    }

    private sealed class TreeFrame<T> : TreeFrame, IGuiTreeBuilder<T>
        where T : IGuiNode, new()
    {
        private readonly GuiTreeBuilder _treeBuilder;

        public override Type ComponentType => typeof(T);

        private Action<T>? _configure;
        private SlotCallbacks _ownCallbacks;
        private SlotCallbacks _externalCallbacks;

        public TreeFrame(GuiTreeBuilder treeBuilder, int key)
        {
            _treeBuilder = treeBuilder;
            Key = key;
        }

        IGuiTreeBuilder<T> IGuiTreeBuilder<T>.AddConfigurationAction(Action<T> action)
        {
            _configure += action;
            return this;
        }

        IGuiSlotBuilder IGuiSlotBuilder.AddLayoutConfiguration(Action<GuiComponentLayoutParameters> configure)
        {
            _configure += node =>
            {
                if (node is not IGuiComponent component)
                {
                    throw new InvalidOperationException(
                        $"Layout parameters cannot be applied to layout-transparent node {typeof(T).Name}.");
                }

                configure(component.LayoutParameters);
            };
            return this;
        }

        IGuiSlotBuilder IGuiSlotBuilder.AddMouseHandler(GuiMouseEventKind kind, GuiCallback<GuiMouseEventArgs> callback)
        {
            _externalCallbacks.AddMouseHandler(kind, callback);
            return this;
        }

        IGuiSlotBuilder IGuiSlotBuilder.AddKeyHandler(GuiKeyEventKind kind, GuiCallback<GuiKeyEventArgs> callback)
        {
            _externalCallbacks.AddKeyHandler(kind, callback);
            return this;
        }

        IGuiSlotBuilder IGuiSlotBuilder.AddFocusChangedHandler(GuiCallback<bool> callback)
        {
            _externalCallbacks.AddFocusChangedHandler(callback);
            return this;
        }

        public override void Reset()
        {
            _configure = null;
            // Mouse + keyboard handlers are also per-pass: each blueprint pass re-registers
            // them via own-slot configuration and the On* extensions, mirroring how Configure
            // actions are re-registered.
            _ownCallbacks = default;
            _externalCallbacks = default;
        }

        public override IGuiNode CreateInstance() => new T();

        public override void ApplyConfiguration(IGuiNode instance)
        {
            if (instance is T typed)
            {
                _configure?.Invoke(typed);
            }
        }

        public override void ApplySlotConfiguration(IGuiNode instance)
        {
            _ownCallbacks = default;
            instance.ConfigureSlot(new SlotBuilder(this, instance));
        }

        public override void ComposeSlotConfiguration(GuiSlot slot)
        {
            SlotCallbacks.Combine(_ownCallbacks, _externalCallbacks).ApplyTo(slot);
        }

        IGuiTreeBuilder<TNewComponent> IGuiTreeBuilder.AddComponent<TNewComponent>(int key)
            => _treeBuilder.AddComponent<TNewComponent>(key);

        void IGuiTreeBuilder.PushCascadeScope<TValue>(TValue value, string? name, GuiTreeFragment content)
            => _treeBuilder.PushCascadeScope(value, name, content);

        private sealed class SlotBuilder(TreeFrame<T> frame, IGuiNode instance) : IGuiSlotBuilder
        {
            IGuiSlotBuilder IGuiSlotBuilder.AddLayoutConfiguration(Action<GuiComponentLayoutParameters> configure)
            {
                if (instance is not IGuiComponent component)
                {
                    throw new InvalidOperationException(
                        $"Layout parameters cannot be applied to layout-transparent node {typeof(T).Name}.");
                }

                configure(component.LayoutParameters);
                return this;
            }

            IGuiSlotBuilder IGuiSlotBuilder.AddMouseHandler(GuiMouseEventKind kind, GuiCallback<GuiMouseEventArgs> callback)
            {
                frame._ownCallbacks.AddMouseHandler(kind, callback);
                return this;
            }

            IGuiSlotBuilder IGuiSlotBuilder.AddKeyHandler(GuiKeyEventKind kind, GuiCallback<GuiKeyEventArgs> callback)
            {
                frame._ownCallbacks.AddKeyHandler(kind, callback);
                return this;
            }

            IGuiSlotBuilder IGuiSlotBuilder.AddFocusChangedHandler(GuiCallback<bool> callback)
            {
                frame._ownCallbacks.AddFocusChangedHandler(callback);
                return this;
            }
        }
    }

    internal abstract class TreeFrame
    {
        public abstract Type ComponentType { get; }
        public int Key { get; protected init; }

        // Snapshotted during the blueprint phase in AddComponent. Records the cascade chain
        // that was active at this slot's declaration site — including any PushCascadeScope
        // wrappers around the AddComponent call. Read during the patch phase to initialise
        // the child builder's CascadeChain before recursing into the slot's subtree.
        public CascadingValueChain? CascadeChain;

        public abstract IGuiNode CreateInstance();
        /// <summary>Clears per-pass state (registered configuration actions and mouse handlers).
        /// Called when an existing frame is reused at the start of a new blueprint pass.</summary>
        public abstract void Reset();
        public abstract void ApplySlotConfiguration(IGuiNode instance);
        public abstract void ApplyConfiguration(IGuiNode instance);
        public abstract void ComposeSlotConfiguration(GuiSlot slot);
    }

    private readonly record struct ComponentSlotKey(Type ComponentType, int Key);

}
