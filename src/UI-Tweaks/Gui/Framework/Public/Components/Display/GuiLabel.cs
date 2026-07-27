using Cairo;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A leaf component that renders a single line of text using <see cref="GuiFontStyle"/>.
/// Resolves its final bounds from the text's intrinsic dimensions.
/// </summary>
public sealed class GuiLabel : GuiComponent
{
    /// <summary>The text to display.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The font style used to render <see cref="Text"/>.
    /// Defaults to <see cref="GuiFontStyle.Default"/>.
    /// </summary>
    public GuiFontStyle Font { get; set; } = GuiFontStyle.Default;

    // ── IGuiComponent ─────────────────────────────────────────────────────

    protected override GuiBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
    {
        GuiSize measuredSize =
            string.IsNullOrEmpty(Text)
                ? new GuiSize(0, 0)
                : Font.Measure(Text);

        return LayoutParameters.ResolveBounds(
            availableBounds,
            new GuiBounds(null, measuredSize));
    }

    public override void Render(Context context, GuiBounds bounds)
    {
        var position = bounds.Position!.Value;

        // DrawText handles the physical-pixel CTM dance required for vanilla-style hinting.
        context.DrawText(Text, Font, position.X, position.Y);
    }
}
