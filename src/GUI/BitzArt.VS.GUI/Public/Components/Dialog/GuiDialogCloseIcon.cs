using Cairo;

namespace BitzArt.VS.GUI;

/// <summary>
/// The "X" icon vanilla shows in the top-right of a dialog title bar. Paints the same
/// cross + drop-shadow as <c>GuiElementDialogTitleBar</c> but reacts to hover by simply
/// swapping the cross colour, instead of vanilla's separately-textured red overlay.
/// <para>
/// Vanilla's overlay is a pre-baked <c>LoadedTexture</c> blitted on top of the bar at
/// hover time; because the texture is generated once at compose time at a slightly
/// different pixel size and is then redrawn at the icon's location with a +4 size
/// fudge, the overlay is not always perfectly aligned with the icon underneath. We
/// avoid that whole mechanism — the icon is drawn directly into the dialog's Cairo
/// surface every frame, and a hover-triggered re-render switches the source colour.
/// </para>
/// <para>
/// Bounds geometry: the visible cross spans <see cref="CrossSize"/> + 2× <see cref="CrossLineWidth"/>
/// logical pixels before display scaling (lines run from <c>(lw, lw)</c> to
/// <c>(lw + size, lw + size)</c> per vanilla <c>IconUtil.DrawCross</c>). The drop shadow
/// extends one extra logical pixel before display scaling
/// past the cross on the bottom-right; the component's allocated bounds are sized to
/// fit the whole thing — a 19×19 box at the default <c>CrossSize = 15</c> +
/// <c>CrossLineWidth = 2</c>. The mouse-hit region matches that bounding box exactly,
/// so hover state never extends past the visible icon.
/// </para>
/// </summary>
public class GuiDialogCloseIcon : GuiComponent
{
    /// <summary>
    /// Visible cross size (length of each diagonal line) in logical pixels before display scaling.
    /// Default 15 — vanilla <c>unscaledCloseIconSize</c>.
    /// </summary>
    public double CrossSize { get; set; } = 15;

    /// <summary>
    /// Cross stroke width in logical pixels before display scaling.
    /// Default 2 — vanilla <c>scaled(2)</c>.
    /// </summary>
    public double CrossLineWidth { get; set; } = 2;

    /// <summary>Idle-state cross colour. Default <see cref="GuiStyle.DialogDefaultTextColor"/>.</summary>
    public GuiColor IconColor { get; set; } = GuiVanillaStyle.DialogDefaultTextColor;

    /// <summary>
    /// Hovered-state cross colour. Default is the bright red vanilla bakes into its
    /// hover texture (<c>(0.8, 0.2, 0.2)</c>). Swapping this at hover is the entire
    /// "weird overlay" replacement — the cross simply re-paints in this colour.
    /// </summary>
    public GuiColor HoverIconColor { get; set; } = GuiColor.FromRgba(0.8, 0.2, 0.2, 1.0);

    /// <summary>Drop-shadow colour painted underneath the cross. Default black at 30% alpha — vanilla.</summary>
    public GuiColor ShadowColor { get; set; } = GuiColor.FromRgba(0.0, 0.0, 0.0, 0.3);

    /// <summary>Click handler. Invoked when the cursor releases inside the icon's bounds after pressing inside.</summary>
    public GuiCallback? OnClick { get; set; }

    // Hover state — flipped by own-slot OnMouseEnter/Leave handlers and read by Render.
    private bool _isHovered;

    /// <summary>
    /// The icon's natural size. Includes the 2-px line-width margin DrawCross adds around
    /// the visible cross (see remarks on the class doc).
    /// </summary>
    protected override GuiComponentBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
    {
        double measuredSize =
            Math.Max(0, CrossLineWidth * 2 + CrossSize);

        return LayoutParameters.ResolveBounds(
            availableBounds,
            new GuiBounds(
                null,
                new GuiSize(measuredSize, measuredSize)));
    }

    public override void Render(Context ctx, GuiBounds b)
    {
        if (Slot is null)
        {
            return;
        }

        var icons = Slot.ClientApi.Gui.Icons;
        var position = b.Position!.Value;

        // CTM is logical pixels here; vanilla's IconUtil.DrawCross expects raw line-width
        // and cross-size values, so we pass them through unchanged.

        // 1. Drop shadow — offset by 1 logical pixel to approximate vanilla's +2 physical-px
        //    shadow at GUIScale=2; using a fixed 1 logical-px offset keeps the shadow visible
        //    and within bounds at any scale.
        ctx.Operator = Operator.Over;
        ctx.SetSourceRGBA(ShadowColor.R, ShadowColor.G, ShadowColor.B, ShadowColor.A);
        icons.DrawCross(ctx, position.X + 1, position.Y + 1, CrossLineWidth, CrossSize);

        // 2. Cross — colour swaps to HoverIconColor while hovered. This is the "no overlay"
        //    behaviour: same geometry as the idle icon, just a different stroke colour, so
        //    nothing can ever fall out of alignment.
        ctx.Operator = Operator.Source;
        var color = _isHovered ? HoverIconColor : IconColor;
        ctx.SetSourceRGBA(color.R, color.G, color.B, color.A);
        icons.DrawCross(ctx, position.X, position.Y, CrossLineWidth, CrossSize);

        // Restore default operator so subsequent siblings paint with the framework's expected
        // blending mode.
        ctx.Operator = Operator.Over;
    }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        base.ConfigureSlot(builder);
        builder
            .OnMouseEnter((Action<GuiMouseEventArgs>)HandleMouseEnter)
            .OnMouseLeave((Action<GuiMouseEventArgs>)HandleMouseLeave)
            .OnMouseClick((Func<GuiMouseEventArgs, ValueTask>)HandleMouseClickAsync);
    }

    private void HandleMouseEnter(GuiMouseEventArgs e)
    {
        _isHovered = true;
        Slot!.RequestRender();
    }

    private void HandleMouseLeave(GuiMouseEventArgs e)
    {
        _isHovered = false;
        Slot!.RequestRender();
    }

    private async ValueTask HandleMouseClickAsync(GuiMouseEventArgs e)
    {
        if (OnClick is GuiCallback onClick)
        {
            await onClick.InvokeAsync();
        }
    }
}
