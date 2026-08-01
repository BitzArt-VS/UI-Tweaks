using Cairo;
using Vintagestory.API.Config;

namespace BitzArt.VS.GUI;

/// <summary>
/// A panel that paints the vanilla "shaded title bar" look — a slightly lighter solid
/// fill plus an open three-sided dark border (left + top + right). The bottom edge is
/// closed by a sibling <see cref="GuiDialogBackground"/> below
/// (its <see cref="GuiDialogBackground.JoinedTopEdge"/> top stroke meets ours).
/// <para>
/// Mirrors vanilla <c>GuiElementDialogTitleBar.ComposeTextElements</c> except for the
/// inset highlight + <c>BlurPartial</c> bevel pass. That pass relies on surface-level
/// blur — without blur, a literal reproduction shows as a sharp inset rectangle that
/// reads worse than no bevel at all, so it's intentionally omitted.
/// </para>
/// <para>
/// The title text is painted directly by <see cref="DrawBackground"/> rather than as a
/// child component so the title bar owns its text painting together with its background
/// and border.
/// </para>
/// </summary>
public class GuiDialogTitleBar : GuiContainer
{
    /// <summary>The title text drawn inside the bar.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Font used to draw <see cref="Title"/>. Defaults to <see cref="GuiFontStyle.Default"/>.</summary>
    public GuiFontStyle TitleFont { get; set; } = GuiFontStyle.Default;

    /// <summary>Horizontal inset of the title text from the left edge in logical pixels. Default: <see cref="GuiVanillaStyle.ElementToDialogPadding"/> (20, vanilla).</summary>
    public double TitleLeftPadding { get; set; } = GuiVanillaStyle.ElementToDialogPadding;

    /// <summary>The base fill colour of the bar. Default: <see cref="GuiVanillaStyle.DialogTitleBarBgColor"/> (vanilla strong bg × 1.2).</summary>
    public GuiColor FillColor { get; set; } = GuiVanillaStyle.DialogTitleBarBgColor;

    /// <summary>
    /// Outer dark border stroke width in <b>physical</b> pixels. Default 2 — matches
    /// <see cref="GuiDialogBackground.StrokeWidth"/> so the title bar and body
    /// share a uniform border weight.
    /// </summary>
    public double StrokeWidth { get; set; } = 2;

    /// <summary>Outer dark border colour. Default: <see cref="GuiVanillaStyle.DialogShadedStrokeColor"/>.</summary>
    public GuiColor BorderColor { get; set; } = GuiVanillaStyle.DialogShadedStrokeColor;

    /// <summary>
    /// Drag callback. When set, the title bar acts as a drag handle: holding the left mouse
    /// button on the bar and moving the cursor invokes this callback once per move event with
    /// the cursor delta in <b>logical (unscaled) pixels</b> since the previous event. Pass
    /// <c>this.Move</c> from a <see cref="GuiDialog"/> subclass to make the title bar drag
    /// the parent dialog around. Default <c>null</c> — drag interaction is disabled.
    /// </summary>
    public Action<double, double>? OnDrag { get; set; }

    /// <summary>
    /// Close callback. When set, the bar paints a vanilla-style "X" icon in its top-right
    /// corner that invokes this callback on click. Default <c>null</c> — no close icon
    /// is drawn.
    /// </summary>
    public GuiCallback? OnClose { get; set; }

    /// <summary>
    /// Right-edge inset of the close icon in logical pixels. Matches vanilla's
    /// <c>scaled(12)</c> spacing between the icon and the bar's right edge.
    /// </summary>
    public double CloseIconRightPadding { get; set; } = 12;

    /// <summary>
    /// Top inset of the close icon in logical pixels. Matches vanilla's <c>scaled(7)</c>.
    /// </summary>
    public double CloseIconTopPadding { get; set; } = 7;

    // Screen-absolute logical-coordinate anchor for drag delta computation. Absolute coords
    // are stable across moves — the dialog-local frame shifts with each position update, but
    // the screen-absolute frame does not, so deltas taken from it are always correct.
    private double _dragLastX;
    private double _dragLastY;
    private bool _dragging;

    // Captured from their declarative slots so Arrange can supply resolved coordinates
    // after the title bar's own content bounds are known.
    private GuiRectangle? _dragTarget;
    private GuiDialogCloseIcon? _closeIcon;

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        base.ConfigureSlot(builder);
        builder.ConfigureLayout(layout =>
        {
            layout.Height = GuiVanillaStyle.TitleBarHeight;
            layout.Width = GuiLengthRule.Fill;
        });
    }

    public override GuiComponentBounds Arrange(GuiBounds availableBounds)
    {
        var provisionalBounds =
            LayoutParameters.ResolveBounds(availableBounds);

        var contentBounds =
            provisionalBounds.ContentBounds;

        var parentPosition =
            availableBounds.Position
            ?? throw new InvalidOperationException(
                "Title-bar content requires a resolved parent position.");

        var contentPosition =
            contentBounds.Position?.Resolve(parentPosition)
            ?? throw new InvalidOperationException(
                "Title-bar content requires a resolved position.");

        _dragTarget?.LayoutParameters.Position =
            contentPosition;

        if (_closeIcon is not null)
        {
            var contentSize =
                contentBounds.Size
                ?? throw new InvalidOperationException(
                    "Title-bar content requires a resolved size.");

            var width =
                contentSize.Width
                ?? throw new InvalidOperationException(
                    "Title-bar content requires a resolved width.");

            var iconBox =
                _closeIcon.CrossLineWidth * 2
                + _closeIcon.CrossSize;

            _closeIcon.LayoutParameters.Position =
                new GuiPoint(
                    contentPosition.X + width - iconBox - CloseIconRightPadding,
                    contentPosition.Y + CloseIconTopPadding,
                    IsAbsolute: true);
        }

        return base.Arrange(availableBounds);
    }

    protected override void DrawBackground(Context ctx, GuiBounds bounds)
    {
        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;
        double sw = StrokeWidth / RuntimeEnv.GUIScale;

        // 1. Solid lighter fill — establishes the title bar's brighter tone vs. the body.
        ctx.RoundRect(position.X, position.Y, width, height, 0);
        ctx.FillSolid(FillColor);

        // 2. Open 3-sided dark border (left + top + right; bottom open). Path is flush
        //    with bounds — Cairo strokes straddle the path, so the outer half of the top
        //    edge is clipped by the dialog surface boundary, leaving the visible border
        //    at half the stroke width. This matches vanilla, where the equivalent clip
        //    happens against the dialog's own surface edge.
        ctx.OpenRect(position.X, position.Y, width, height, GuiEdge.Bottom);
        ctx.StrokeSolid(BorderColor, sw);

        // 3. Title text — vertically centred inside the bar, left-aligned with vanilla padding.
        if (!string.IsNullOrEmpty(Title))
        {
            double textHeight = TitleFont.MeasureHeight();
            double textY = position.Y + (height - textHeight) / 2.0;
            ctx.DrawText(Title, TitleFont, position.X + TitleLeftPadding, textY);
        }

    }

    protected override void BuildComponentTree(IGuiTreeBuilder builder)
    {
        // Render any user-supplied content first (matches the GuiContainer contract).
        base.BuildComponentTree(builder);

        // Drag click-target — only emitted when an OnDrag handler is attached. An absolute,
        // fill-mode container covers the title bar's entire content area without participating
        // in flow layout (so it doesn't push Content children around) and registers the
        // mouse handlers that drive drag. Pattern mirrors GuiButton's inner click target.
        //
        // Emitted *before* the close icon below so the icon's interactive region is added to
        // the renderer's region table last; hit-testing walks the table in reverse, so the
        // smaller close-icon region wins over the full-bar drag region when both contain the
        // cursor.
        if (OnDrag is not null)
        {
            builder.Add<GuiMouseTarget>(int.MaxValue - 1)
                .Configure(target => target.Content = BuildDragTargetContent)
                .OnMouseDown((Action<GuiMouseEventArgs>)HandleMouseDown)
                .OnMouseMove((Action<GuiMouseEventArgs>)HandleMouseMove)
                .OnMouseUp((Action<GuiMouseEventArgs>)HandleMouseUp);
        }
        else
        {
            _dragTarget = null;
        }

        // Configure captures the live close icon so Arrange can place it from the resolved
        // title-bar content bounds.
        if (OnClose is not null)
        {
            builder.Add<GuiDialogCloseIcon>(int.MaxValue)
                .Configure(icon =>
                {
                    icon.OnClick = OnClose;
                    _closeIcon = icon;
                });
        }
        else
        {
            _closeIcon = null;
        }
    }

    private void BuildDragTargetContent(IGuiTreeBuilder builder)
    {
        builder.Add<GuiRectangle>(0)
            .Configure(rectangle =>
                _dragTarget = rectangle)
            .ConfigureLayout(layout =>
            {
                layout.Width = GuiLengthRule.Fill;
                layout.Height = GuiLengthRule.Fill;
            });
    }

    private void HandleMouseDown(GuiMouseEventArgs e)
    {
        // Only the left mouse button initiates a drag — matches vanilla title-bar behaviour.
        if (e.Button != Vintagestory.API.Common.EnumMouseButton.Left)
        {
            return;
        }

        _dragging = true;
        _dragLastX = e.AbsolutePosition.X;
        _dragLastY = e.AbsolutePosition.Y;
    }

    private void HandleMouseMove(GuiMouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        double dx = e.AbsolutePosition.X - _dragLastX;
        double dy = e.AbsolutePosition.Y - _dragLastY;
        _dragLastX = e.AbsolutePosition.X;
        _dragLastY = e.AbsolutePosition.Y;

        if (dx == 0 && dy == 0)
        {
            return;
        }

        OnDrag?.Invoke(dx, dy);
    }

    private void HandleMouseUp(GuiMouseEventArgs e)
    {
        _dragging = false;
    }
}
