using Cairo;
using Vintagestory.API.Config;

namespace BitzArt.VS.GUI;

/// <summary>
/// The framework's general-purpose layout/surface component — analogous to a
/// <c>&lt;div&gt;</c> in HTML or <c>Container</c> in Flutter. Hosts a nested render tree
/// declared via <see cref="Content"/>, flows children from left to right with line
/// wrapping, and optionally paints a background fill.
/// <para>
/// A <see langword="null"/> width or height fits content. Set an explicit
/// <see cref="GuiLength"/> to use a fixed or fractional size.
/// </para>
/// <para>
/// <b>Drawing.</b> The painting pass is split into two overrideable hooks called by the
/// framework in order:
/// <list type="number">
///   <item><see cref="DrawBackground"/> — before children. Default: fills bounds with
///   <see cref="Background"/>; no-op when <see cref="Background"/> is fully transparent.</item>
///   <item><see cref="DrawOverlay"/> — after all children. Default: no-op.</item>
/// </list>
/// Subclass and override these for chrome (textures, borders, glows, etc.). The base
/// <c>Render</c> / <c>RenderOverlay</c> are sealed to keep the two-hook contract uniform
/// across every container subtype.
/// </para>
/// <para>
/// <b>Scrolling.</b> Set <see cref="Scroll"/> to enable scrolling on one or both axes.
/// When enabled, the container clips its content tree to the viewport and translates
/// children by the current scroll offset. A <see langword="null"/> size on a given axis
/// disables scrolling on that axis because a fit-content container has no overflow.
/// Scrollbars are visible when content overflows the viewport (filtered by
/// <see cref="Scrollbar"/>) or when forced via <see cref="AlwaysShowScrollbar"/>.
/// </para>
/// </summary>
public class GuiContainer : GuiComponent
{
    /// <summary>
    /// The nested render fragment that populates this container's inner content.
    /// Set through <c>.Configure(container =&gt; container.Content = ...)</c>.
    /// </summary>
    public GuiTreeFragment? Content { get; set; }

    /// <summary>
    /// Background fill colour. Defaults to fully transparent — the default
    /// <see cref="DrawBackground"/> short-circuits when alpha is zero, so a plain
    /// <c>GuiContainer</c> draws nothing and behaves as an invisible flow box.
    /// </summary>
    public GuiColor Background { get; set; }

    // ── Scrolling ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Axes on which scrolling is enabled. Defaults to <see cref="GuiDirection.None"/>.
    /// An axis whose corresponding width or height is <see langword="null"/> is silently
    /// ignored because a fit-content container has no overflow.
    /// </summary>
    public GuiDirection Scroll { get; set; } = GuiDirection.None;

    /// <summary>
    /// Axes for which a scrollbar may be displayed. Defaults to <see cref="GuiDirection.Both"/>.
    /// A scrollbar is only shown for an axis when (a) that axis is included here, AND
    /// (b) the axis is included in <see cref="Scroll"/>, AND (c) either content overflows
    /// along that axis or <see cref="AlwaysShowScrollbar"/> includes that axis.
    /// </summary>
    public GuiDirection Scrollbar { get; set; } = GuiDirection.Both;

    /// <summary>
    /// Axes for which the scrollbar should remain visible even when content does not
    /// overflow. Defaults to <see cref="GuiDirection.None"/>. Has no effect for axes not
    /// included in <see cref="Scroll"/> / <see cref="Scrollbar"/>.
    /// </summary>
    public GuiDirection AlwaysShowScrollbar { get; set; } = GuiDirection.None;

    // ── Inset chrome ──────────────────────────────────────────────────────────

    /// <summary>
    /// When <c>true</c>, the container draws an owned <see cref="GuiInset"/> around its
    /// content viewport, producing the vanilla recessed-border look. Defaults to
    /// <c>false</c>. Use <see cref="InsetConfiguration"/> to adjust the inset's
    /// <see cref="GuiInset.Depth"/> / <see cref="GuiInset.Brightness"/> /
    /// <see cref="GuiInset.Radius"/> without subclassing.
    /// </summary>
    public bool HasInset { get; set; }

    /// <summary>
    /// Optional callback that configures the owned <see cref="GuiInset"/> drawn when
    /// <see cref="HasInset"/> is set. Lets callers customise the inset visual without
    /// subclassing the container.
    /// </summary>
    public Action<GuiInset>? InsetConfiguration { get; set; }

    /// <summary>Current horizontal scroll offset in logical pixels before display scaling. Mutate via user input
    /// (mouse wheel / scrollbar drag) or <see cref="ScrollTo"/>.</summary>
    public double ScrollX { get; private set; }

    /// <summary>Current vertical scroll offset in logical pixels before display scaling. Mutate via user input
    /// (mouse wheel / scrollbar drag) or <see cref="ScrollTo"/>.</summary>
    public double ScrollY { get; private set; }

    /// <summary>
    /// Sets scroll offsets explicitly. Values are clamped to the valid range during the next
    /// arrangement. Pass a negative value to leave an axis unchanged.
    /// </summary>
    public void ScrollTo(double scrollX, double scrollY)
    {
        double previousScrollX = ScrollX;
        double previousScrollY = ScrollY;
        if (scrollX >= 0)
        {
            ScrollX = scrollX;
        }

        if (scrollY >= 0)
        {
            ScrollY = scrollY;
        }

        if (ScrollX != previousScrollX || ScrollY != previousScrollY)
        {
            Slot!.RequestArrange();
        }
    }

    // ── Drawing hooks ─────────────────────────────────────────────────────────

    /// <summary>
    /// Override to draw the container's background. Called before children are rendered.
    /// Default: fills bounds with a solid <see cref="Background"/> when its alpha is
    /// greater than zero; otherwise no-op.
    /// </summary>
    protected virtual void DrawBackground(Context context, GuiBounds bounds)
    {
        if (Background.A <= 0)
        {
            return;
        }

        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;

        context.SetSourceRGBA(Background.R, Background.G, Background.B, Background.A);
        context.Rectangle(
            position.X,
            position.Y,
            size.Width!.Value,
            size.Height!.Value);
        context.Fill();
    }

    /// <summary>
    /// Override to draw overlays on top of children (borders, highlights, etc.).
    /// Called after all children are rendered.
    /// Default: no-op.
    /// </summary>
    protected virtual void DrawOverlay(Context context, GuiBounds bounds) { }

    // ── Framework wiring ──────────────────────────────────────────────────────

    private ClippingContext? _clippingContext;

    public override GuiComponentBounds Arrange(GuiBounds availableBounds)
    {
        var availablePosition =
            availableBounds.Position
            ?? throw new InvalidOperationException(
                "Scrollable container requires an available position.");

        SetMouseTargetBounds(
            _scrollWheelTarget,
            bounds: null,
            availablePosition);

        SetMouseTargetBounds(
            _verticalScrollbarTarget,
            bounds: null,
            availablePosition);

        SetMouseTargetBounds(
            _horizontalScrollbarTarget,
            bounds: null,
            availablePosition);

        var arrangedBounds =
            base.Arrange(availableBounds);

        var slot = (GuiComponentSlot)Slot!;
        var resolvedComponentBounds =
            slot.Bounds
            ?? throw new InvalidOperationException(
                "Scrollable container requires resolved bounds.");

        var resolvedBounds =
            resolvedComponentBounds.Bounds;

        var viewportBounds =
            resolvedComponentBounds.ContentBounds;

        _effectiveScroll = ResolveEffectiveScroll();
        if (_effectiveScroll == GuiDirection.None)
        {
            _showVScrollbar = false;
            _showHScrollbar = false;
            SetContentClip(bounds: null);
            return arrangedBounds;
        }

        var viewportPosition =
            viewportBounds.Position
            ?? throw new InvalidOperationException(
                "Scrollable container requires a resolved viewport position.");

        var viewportSize =
            viewportBounds.Size
            ?? throw new InvalidOperationException(
                "Scrollable container requires a resolved viewport size.");

        var viewportWidth =
            viewportSize.Width
            ?? throw new InvalidOperationException(
                "Scrollable container requires a resolved viewport width.");

        var viewportHeight =
            viewportSize.Height
            ?? throw new InvalidOperationException(
                "Scrollable container requires a resolved viewport height.");

        var wantVerticalScrollbar =
            (_effectiveScroll & GuiDirection.Vertical) != 0
            && (Scrollbar & GuiDirection.Vertical) != 0;

        var wantHorizontalScrollbar =
            (_effectiveScroll & GuiDirection.Horizontal) != 0
            && (Scrollbar & GuiDirection.Horizontal) != 0;

        var showVerticalScrollbar =
            wantVerticalScrollbar
            && (AlwaysShowScrollbar & GuiDirection.Vertical) != 0;

        var showHorizontalScrollbar =
            wantHorizontalScrollbar
            && (AlwaysShowScrollbar & GuiDirection.Horizontal) != 0;

        var scrollbarReserve =
            ScrollbarThickness + ScrollbarGap;

        double resolvedViewportWidth;
        double resolvedViewportHeight;
        double contentWidth;
        double contentHeight;

        // Scrollbar visibility only grows during stabilization. With two axes this
        // reaches a fixed point after at most two visibility changes.
        while (true)
        {
            resolvedViewportWidth =
                Math.Max(
                    0,
                    viewportWidth
                    - (showVerticalScrollbar ? scrollbarReserve : 0));

            resolvedViewportHeight =
                Math.Max(
                    0,
                    viewportHeight
                    - (showHorizontalScrollbar ? scrollbarReserve : 0));

            viewportBounds =
                viewportBounds with
                {
                    Size = new GuiSize(
                        resolvedViewportWidth,
                        resolvedViewportHeight),
                };

            var contentSize =
                ResolveContentSize(
                    slot.ArrangeChildren(viewportBounds));

            contentWidth =
                contentSize.Width ?? 0;

            contentHeight =
                contentSize.Height ?? 0;

            var stabilizedVerticalScrollbar =
                showVerticalScrollbar
                || (wantVerticalScrollbar
                    && contentHeight > resolvedViewportHeight + 0.5);

            var stabilizedHorizontalScrollbar =
                showHorizontalScrollbar
                || (wantHorizontalScrollbar
                    && contentWidth > resolvedViewportWidth + 0.5);

            if (stabilizedVerticalScrollbar == showVerticalScrollbar
                && stabilizedHorizontalScrollbar == showHorizontalScrollbar)
            {
                break;
            }

            showVerticalScrollbar = stabilizedVerticalScrollbar;
            showHorizontalScrollbar = stabilizedHorizontalScrollbar;
        }

        UpdateScrollLayout(
            resolvedBounds.Position!.Value.X,
            resolvedBounds.Position.Value.Y,
            resolvedBounds.Size!.Value.Width!.Value,
            resolvedBounds.Size.Value.Height!.Value,
            viewportPosition.X,
            viewportPosition.Y,
            resolvedViewportWidth,
            resolvedViewportHeight,
            contentWidth,
            contentHeight,
            showVerticalScrollbar,
            showHorizontalScrollbar,
            ScrollbarThickness);

        SetMouseTargetBounds(
            _scrollWheelTarget,
            viewportBounds,
            viewportPosition);

        SetMouseTargetBounds(
            _verticalScrollbarTarget,
            showVerticalScrollbar
                ? GetVScrollbarTrackBounds()
                : null,
            viewportPosition);

        SetMouseTargetBounds(
            _horizontalScrollbarTarget,
            showHorizontalScrollbar
                ? GetHScrollbarTrackBounds()
                : null,
            viewportPosition);

        var clipBounds =
            viewportBounds.Contract(
                new GuiThickness(
                    ScrollViewportClipInset));

        SetContentClip(clipBounds);

        var scrolledContentBounds =
            new GuiBounds(
                new GuiPoint(
                    viewportPosition.X - ScrollX,
                    viewportPosition.Y - ScrollY,
                    IsAbsolute: true),
                new GuiSize(
                    (_effectiveScroll & GuiDirection.Horizontal) != 0
                        ? Math.Max(resolvedViewportWidth, contentWidth)
                        : resolvedViewportWidth,
                    (_effectiveScroll & GuiDirection.Vertical) != 0
                        ? Math.Max(resolvedViewportHeight, contentHeight)
                        : resolvedViewportHeight));

        slot.ArrangeChildren(scrolledContentBounds);
        return arrangedBounds;
    }

    private GuiDirection ResolveEffectiveScroll()
    {
        var effectiveScroll = Scroll;

        if (LayoutParameters.Width is null)
        {
            effectiveScroll &= ~GuiDirection.Horizontal;
        }

        if (LayoutParameters.Height is null)
        {
            effectiveScroll &= ~GuiDirection.Vertical;
        }

        return effectiveScroll;
    }

    private static GuiSize ResolveContentSize(GuiBounds? descendantsBounds)
    {
        if (descendantsBounds is not GuiBounds bounds)
        {
            return new GuiSize(0, 0);
        }

        var size =
            bounds.Size
            ?? new GuiSize(0, 0);

        if (bounds.Position is not GuiPoint position
            || position.IsAbsolute)
        {
            return size;
        }

        return new GuiSize(
            position.X + size.Width,
            position.Y + size.Height);
    }

    private void SetContentClip(GuiBounds? bounds)
    {
        if (_clippingContext is not null
            && _scrollContentScope is not null)
        {
            _clippingContext.SetClip(_scrollContentScope, bounds);
        }
    }

    /// <summary>
    /// Renders the nested <see cref="Content"/> fragment into this container. Subclasses may
    /// override to inject additional children (e.g. an overlay click target); call
    /// <c>base.BuildComponentTree(builder)</c> to keep <see cref="Content"/> support.
    /// </summary>
    protected override void BuildComponentTree(IGuiTreeBuilder builder)
    {
        _scrollWheelTarget = null;
        _scrollContentScope = null;
        _verticalScrollbarTarget = null;
        _horizontalScrollbarTarget = null;

        if (Scroll == GuiDirection.None)
        {
            Content?.Invoke(builder);
            return;
        }

        builder.Add<ScrollWheelTarget>(int.MinValue)
            .Configure(target =>
            {
                target.Owner = this;
                _scrollWheelTarget = target;
            });

        builder.Add<ScrollContentScope>(int.MinValue + 1)
            .Configure(scope =>
            {
                scope.Content = Content;
                _scrollContentScope = scope;
            });

        builder.Add<ScrollbarMouseTarget>(int.MinValue + 2)
            .Configure(target =>
            {
                target.Owner = this;
                target.IsVertical = true;
                _verticalScrollbarTarget = target;
            });

        builder.Add<ScrollbarMouseTarget>(int.MinValue + 3)
            .Configure(target =>
            {
                target.Owner = this;
                target.IsVertical = false;
                _horizontalScrollbarTarget = target;
            });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Forwards <see cref="InsetConfiguration"/> (when set) to the container's owned
    /// <see cref="GuiInset"/> instance once per reconcile, not per frame — the inset's
    /// own properties stay stable between configuration changes.
    /// </remarks>
    public override void OnParametersSet()
    {
        base.OnParametersSet();
        _clippingContext = GetCascadingValue<ClippingContext>();
        InsetConfiguration?.Invoke(_inset);
    }

    public sealed override void Render(Context context, GuiBounds bounds)
    {
        DrawBackground(context, bounds);
        DrawInsetBackground(
            context,
            _effectiveScroll == GuiDirection.None
                ? bounds
                : GetScrollInsetBounds());
    }

    public sealed override void RenderOverlay(Context context, GuiBounds bounds)
    {
        RenderScrollbars(context);
        DrawOverlay(context, bounds);
    }

    /// <summary>
    /// Owned inset instance configured via <see cref="InsetConfiguration"/> and drawn by
    /// this container when <see cref="HasInset"/> is set. Single instance per container.
    /// </summary>
    private readonly GuiInset _inset = new();

    /// <summary>
    /// Draws the inset background into <paramref name="context"/> at <paramref name="bounds"/>
    /// when <see cref="HasInset"/> is set; no-op otherwise.
    /// </summary>
    private void DrawInsetBackground(Context context, GuiBounds bounds)
    {
        if (!HasInset)
        {
            return;
        }

        GuiInset.Draw(context, bounds, _inset.Depth, _inset.Brightness, _inset.Radius);
    }

    // ── Scrollbar layout / drawing / interaction ─────────────────────────────

    /// <summary>
    /// Default scrollbar thickness in logical pixels before display scaling. Mirrors vanilla
    /// <c>GuiElementScrollbar.DefaultScrollbarWidth</c> (20).
    /// </summary>
    public const double ScrollbarThickness = 20;

    /// <summary>
    /// Gap reserved between the scrollable viewport and the scrollbar track in logical
    /// pixels before display scaling —
    /// keeps content from butting up against the scrollbar handle. The scrollbar itself
    /// still sits flush against the container's allocated edge.
    /// </summary>
    public const double ScrollbarGap = 2;

    /// <summary>Minimum scrollbar handle length in logical pixels before display scaling — keeps the handle
    /// grabbable when content vastly exceeds the viewport. Matches vanilla.</summary>
    private const double MinHandleLength = 10;

    /// <summary>
    /// Approximate scroll distance per mouse-wheel notch in logical pixels before display scaling.
    /// </summary>
    private const double WheelStep = 30;

    /// <summary>
    /// Per-side inset of the scrollbar handle relative to the track. Renders the handle
    /// 2*<see cref="HandleInset"/> pixels narrower than the track so the recessed track
    /// frame stays visible around the handle — small departure from vanilla, intentionally
    /// so.
    /// </summary>
    private const double HandleInset = 1;

    // Requested scroll axes after fit-content dimensions have been filtered out.
    private GuiDirection _effectiveScroll;

    // Cached layout state for the most recent frame, used by RenderScrollbars and the
    // scrollbar mouse handlers. All values in dialog-local logical pixels.
    private double _allocatedX, _allocatedY, _allocatedW, _allocatedH;
    private double _viewportX, _viewportY, _viewportW, _viewportH;
    private double _contentW, _contentH;
    private bool _showVScrollbar, _showHScrollbar;
    private double _sbThickness;

    // Drag state for scrollbar handles. Tracked independently per axis. Only one is active
    // at a time in practice; mouse capture matches by token, not by axis.
    private bool _vDragging;
    private bool _hDragging;
    // Offset from the handle's origin (top for V, left for H) to the click point at
    // drag start. Preserves the grab position as the cursor moves.
    private double _vDragHandleOffset;
    private double _hDragHandleOffset;

    private ScrollbarMouseTarget? _verticalScrollbarTarget;
    private ScrollbarMouseTarget? _horizontalScrollbarTarget;
    private ScrollWheelTarget? _scrollWheelTarget;
    private ScrollContentScope? _scrollContentScope;

    /// <summary>
    /// Caches current component-owned scrolling geometry and clamps offsets to its range.
    /// </summary>
    private void UpdateScrollLayout(
        double allocatedX, double allocatedY, double allocatedW, double allocatedH,
        double viewportX, double viewportY, double viewportW, double viewportH,
        double contentW, double contentH,
        bool showV, bool showH, double sbThickness)
    {
        _allocatedX = allocatedX;
        _allocatedY = allocatedY;
        _allocatedW = allocatedW;
        _allocatedH = allocatedH;
        _viewportX = viewportX;
        _viewportY = viewportY;
        _viewportW = viewportW;
        _viewportH = viewportH;
        _contentW = contentW;
        _contentH = contentH;
        _showVScrollbar = showV;
        _showHScrollbar = showH;
        _sbThickness = sbThickness;

        double maxX = Math.Max(0, contentW - viewportW);
        double maxY = Math.Max(0, contentH - viewportH);
        if (ScrollX > maxX)
        {
            ScrollX = maxX;
        }

        if (ScrollY > maxY)
        {
            ScrollY = maxY;
        }

        if (ScrollX < 0)
        {
            ScrollX = 0;
        }

        if (ScrollY < 0)
        {
            ScrollY = 0;
        }
    }

    /// <summary>
    /// Routes a mouse-wheel event hit on this container's viewport. Vertical wheel scrolls
    /// the vertical axis when enabled, falling back to horizontal otherwise.
    /// </summary>
    private void HandleMouseWheel(float deltaPrecise)
    {
        double previousScrollX = ScrollX;
        double previousScrollY = ScrollY;
        double maxScrollX = Math.Max(0, _contentW - _viewportW);
        double maxScrollY = Math.Max(0, _contentH - _viewportH);

        if ((_effectiveScroll & GuiDirection.Vertical) != 0
            && maxScrollY > 0)
        {
            ScrollY = Clamp(
                ScrollY - deltaPrecise * WheelStep,
                0,
                maxScrollY);
        }
        else if ((_effectiveScroll & GuiDirection.Horizontal) != 0
            && maxScrollX > 0)
        {
            ScrollX = Clamp(
                ScrollX - deltaPrecise * WheelStep,
                0,
                maxScrollX);
        }

        if (ScrollX != previousScrollX || ScrollY != previousScrollY)
        {
            Slot!.RequestArrange();
        }
    }

    private static void SetMouseTargetBounds(
        GuiComponent? target,
        GuiBounds? bounds,
        GuiPoint fallbackPosition)
    {
        if (target is null)
        {
            return;
        }

        var position =
            bounds?.Position
            ?? fallbackPosition;

        var size =
            bounds?.Size
            ?? new GuiSize(0, 0);

        target.LayoutParameters.Position = position;
        target.LayoutParameters.Width = size.Width ?? 0;
        target.LayoutParameters.Height = size.Height ?? 0;
    }

    /// <summary>
    /// Draws visible scrollbars after children and before <see cref="DrawOverlay"/>.
    /// Mirrors vanilla <c>GuiElementScrollbar</c>.
    /// </summary>
    private void RenderScrollbars(Context ctx)
    {
        if (_showVScrollbar)
        {
            var track = GetVScrollbarTrackBounds();
            var trackPosition = track.Position!.Value;
            var trackSize = track.Size!.Value;
            DrawScrollbarTrack(ctx, track);
            var (handleY, handleH) = GetVHandleSpan();
            DrawScrollbarHandle(
                ctx,
                new GuiBounds(
                    new GuiPoint(
                        trackPosition.X + HandleInset,
                        handleY,
                        IsAbsolute: true),
                    new GuiSize(trackSize.Width - 2 * HandleInset, handleH)));
        }
        if (_showHScrollbar)
        {
            var track = GetHScrollbarTrackBounds();
            var trackPosition = track.Position!.Value;
            var trackSize = track.Size!.Value;
            DrawScrollbarTrack(ctx, track);
            var (handleX, handleW) = GetHHandleSpan();
            DrawScrollbarHandle(
                ctx,
                new GuiBounds(
                    new GuiPoint(
                        handleX,
                        trackPosition.Y + HandleInset,
                        IsAbsolute: true),
                    new GuiSize(handleW, trackSize.Height - 2 * HandleInset)));
        }
    }

    /// <summary>Vertical scrollbar track bounds (dialog-local logical px). Anchored
    /// against the container's allocated right edge, with the gap reserved between
    /// viewport and track on the inner side.</summary>
    private GuiBounds GetVScrollbarTrackBounds()
        => new(
            new GuiPoint(
                _allocatedX + _allocatedW - _sbThickness,
                _viewportY,
                IsAbsolute: true),
            new GuiSize(
                _sbThickness,
                _viewportH));

    /// <summary>Horizontal scrollbar track bounds (dialog-local logical px). Anchored
    /// against the container's allocated bottom edge.</summary>
    private GuiBounds GetHScrollbarTrackBounds()
        => new(
            new GuiPoint(
                _viewportX,
                _allocatedY + _allocatedH - _sbThickness,
                IsAbsolute: true),
            new GuiSize(
                _viewportW,
                _sbThickness));

    /// <summary>
    /// Bounds the inset background occupies when scrollbars are visible — the container's
    /// allocated area minus the scrollbar gutter <em>and</em> the gap on each visible
    /// scrollbar axis. Yielding the gap as well as the track keeps a visible separation
    /// between the inset's emboss and the scrollbar track, so they don't read as one
    /// continuous dark strip.
    /// </summary>
    private GuiBounds GetScrollInsetBounds()
        => new(
            new GuiPoint(
                _allocatedX,
                _allocatedY,
                IsAbsolute: true),
            new GuiSize(
                _allocatedW - (_showVScrollbar ? _sbThickness + ScrollbarGap : 0),
                _allocatedH - (_showHScrollbar ? _sbThickness + ScrollbarGap : 0)));

    /// <summary>
    /// Per-side inset in logical pixels before display scaling to apply to the scroll viewport clip region when
    /// <see cref="HasInset"/> is set, so scrollable content is clipped before it reaches
    /// the emboss ring and cannot paint over it.
    /// </summary>
    private double ScrollViewportClipInset => HasInset ? _inset.Depth / RuntimeEnv.GUIScale : 0;

    private static void DrawScrollbarTrack(Context ctx, GuiBounds b)
    {
        // Vanilla composer paints a recessed inset behind the scrollbar handle. Reuse the
        // shared GuiInset visual rather than approximating it — keeps the scrollbar look
        // consistent with the container's own optional inset chrome.
        GuiInset.Draw(ctx, b,
            depth: 4,
            brightness: 0.85f,
            radius: GuiVanillaStyle.ElementBgRadius);
    }

    private static void DrawScrollbarHandle(Context ctx, GuiBounds b)
    {
        var position = b.Position!.Value;
        var size = b.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        // Two-pass fill mirroring vanilla GuiElementScrollbar.RecomposeHandle:
        // 1) DialogHighlightColor base, 2) 40% black wash for depth.
        var hl = GuiVanillaStyle.DialogHighlightColor;
        ctx.Rectangle(position.X, position.Y, width, height);
        ctx.SetSourceRGBA(hl.R, hl.G, hl.B, hl.A);
        ctx.Fill();

        ctx.Rectangle(position.X, position.Y, width, height);
        ctx.SetSourceRGBA(0, 0, 0, 0.4);
        ctx.Fill();

        // Lightweight emboss: 1px top/left highlight + bottom/right shadow.
        ctx.Rectangle(position.X, position.Y, width - 1, 1);
        ctx.SetSourceRGBA(1, 1, 1, 0.18);
        ctx.Fill();
        ctx.Rectangle(position.X, position.Y + 1, 1, height - 1);
        ctx.SetSourceRGBA(1, 1, 1, 0.18);
        ctx.Fill();
        ctx.Rectangle(position.X + 1, position.Y + height - 1, width - 1, 1);
        ctx.SetSourceRGBA(0, 0, 0, 0.25);
        ctx.Fill();
        ctx.Rectangle(position.X + width - 1, position.Y, 1, height - 1);
        ctx.SetSourceRGBA(0, 0, 0, 0.25);
        ctx.Fill();
    }

    private (double Y, double H) GetVHandleSpan()
    {
        double trackH = _viewportH;
        double ratio = _contentH > 0 ? Math.Min(1, _viewportH / _contentH) : 1;
        double handleH = Math.Max(MinHandleLength, ratio * trackH);
        if (handleH > trackH)
        {
            handleH = trackH;
        }

        double scrollable = Math.Max(0, trackH - handleH);
        double maxScroll = Math.Max(0, _contentH - _viewportH);
        double handleY = _viewportY + (maxScroll > 0 ? ScrollY / maxScroll * scrollable : 0);
        return (handleY, handleH);
    }

    private (double X, double W) GetHHandleSpan()
    {
        double trackW = _viewportW;
        double ratio = _contentW > 0 ? Math.Min(1, _viewportW / _contentW) : 1;
        double handleW = Math.Max(MinHandleLength, ratio * trackW);
        if (handleW > trackW)
        {
            handleW = trackW;
        }

        double scrollable = Math.Max(0, trackW - handleW);
        double maxScroll = Math.Max(0, _contentW - _viewportW);
        double handleX = _viewportX + (maxScroll > 0 ? ScrollX / maxScroll * scrollable : 0);
        return (handleX, handleW);
    }

    private void HandleVScrollbarDown(GuiMouseEventArgs e)
    {
        if (!_showVScrollbar)
        {
            return;
        }

        double maxScroll = Math.Max(0, _contentH - _viewportH);
        if (maxScroll <= 0)
        {
            return; // forced-visible scrollbar with no overflow — non-interactive.
        }

        var (handleY, handleH) = GetVHandleSpan();
        if (e.Position.Y >= handleY && e.Position.Y < handleY + handleH)
        {
            // Grab inside handle: keep grab offset so the handle doesn't jump under the cursor.
            _vDragHandleOffset = e.Position.Y - handleY;
        }
        else
        {
            // Track click: jump so the handle is centred on the click, then continue dragging.
            _vDragHandleOffset = handleH / 2.0;
            ApplyVHandlePos(e.Position.Y - _vDragHandleOffset);
        }
        _vDragging = true;
    }

    private void HandleVScrollbarMove(GuiMouseEventArgs e)
    {
        if (!_vDragging)
        {
            return;
        }

        ApplyVHandlePos(e.Position.Y - _vDragHandleOffset);
    }

    private void HandleVScrollbarUp(GuiMouseEventArgs _) => _vDragging = false;

    private void HandleHScrollbarDown(GuiMouseEventArgs e)
    {
        if (!_showHScrollbar)
        {
            return;
        }

        double maxScroll = Math.Max(0, _contentW - _viewportW);
        if (maxScroll <= 0)
        {
            return;
        }

        var (handleX, handleW) = GetHHandleSpan();
        if (e.Position.X >= handleX && e.Position.X < handleX + handleW)
        {
            _hDragHandleOffset = e.Position.X - handleX;
        }
        else
        {
            _hDragHandleOffset = handleW / 2.0;
            ApplyHHandlePos(e.Position.X - _hDragHandleOffset);
        }
        _hDragging = true;
    }

    private void HandleHScrollbarMove(GuiMouseEventArgs e)
    {
        if (!_hDragging)
        {
            return;
        }

        ApplyHHandlePos(e.Position.X - _hDragHandleOffset);
    }

    private void HandleHScrollbarUp(GuiMouseEventArgs _) => _hDragging = false;

    private void ApplyVHandlePos(double newHandleY)
    {
        double previousScrollY = ScrollY;
        var (_, handleH) = GetVHandleSpan();
        double scrollable = Math.Max(0, _viewportH - handleH);
        double maxScroll = Math.Max(0, _contentH - _viewportH);
        double rel = scrollable > 0 ? Clamp((newHandleY - _viewportY) / scrollable, 0, 1) : 0;
        ScrollY = rel * maxScroll;
        if (ScrollY != previousScrollY)
        {
            Slot!.RequestArrange();
        }
    }

    private void ApplyHHandlePos(double newHandleX)
    {
        double previousScrollX = ScrollX;
        var (_, handleW) = GetHHandleSpan();
        double scrollable = Math.Max(0, _viewportW - handleW);
        double maxScroll = Math.Max(0, _contentW - _viewportW);
        double rel = scrollable > 0 ? Clamp((newHandleX - _viewportX) / scrollable, 0, 1) : 0;
        ScrollX = rel * maxScroll;
        if (ScrollX != previousScrollX)
        {
            Slot!.RequestArrange();
        }
    }

    private static double Clamp(double v, double lo, double hi)
        => v < lo ? lo : (v > hi ? hi : v);

    private sealed class ScrollContentScope : GuiNode
    {
        public GuiTreeFragment? Content { get; set; }

        public ScrollContentScope() { }

        protected override void BuildComponentTree(IGuiTreeBuilder builder)
            => Content?.Invoke(builder);
    }

    private sealed class ScrollWheelTarget : GuiComponent
    {
        public GuiContainer? Owner { get; set; }

        public ScrollWheelTarget() { }

        protected override void ConfigureSlot(IGuiSlotBuilder builder)
        {
            base.ConfigureSlot(builder);
            builder.OnMouseWheel((Action<GuiMouseEventArgs>)HandleMouseWheel);
        }

        private void HandleMouseWheel(GuiMouseEventArgs args)
            => Owner?.HandleMouseWheel(args.WheelDelta);
    }

    private sealed class ScrollbarMouseTarget : GuiComponent
    {
        public GuiContainer? Owner { get; set; }
        public bool IsVertical { get; set; }

        public ScrollbarMouseTarget() { }

        protected override void ConfigureSlot(IGuiSlotBuilder builder)
        {
            base.ConfigureSlot(builder);
            builder
                .OnMouseDown((Action<GuiMouseEventArgs>)HandleMouseDown)
                .OnMouseUp((Action<GuiMouseEventArgs>)HandleMouseUp)
                .OnMouseMove((Action<GuiMouseEventArgs>)HandleMouseMove);
        }

        private void HandleMouseDown(GuiMouseEventArgs args)
        {
            if (IsVertical)
            {
                Owner?.HandleVScrollbarDown(args);
            }
            else
            {
                Owner?.HandleHScrollbarDown(args);
            }
        }

        private void HandleMouseUp(GuiMouseEventArgs args)
        {
            if (IsVertical)
            {
                Owner?.HandleVScrollbarUp(args);
            }
            else
            {
                Owner?.HandleHScrollbarUp(args);
            }
        }

        private void HandleMouseMove(GuiMouseEventArgs args)
        {
            if (IsVertical)
            {
                Owner?.HandleVScrollbarMove(args);
            }
            else
            {
                Owner?.HandleHScrollbarMove(args);
            }
        }
    }
}
