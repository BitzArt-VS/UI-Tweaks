using BitzArt.UI.Tweaks.Gui;
using Cairo;

namespace BitzArt.UI.Tweaks;

internal sealed class ConfigPanel : GuiContainer
{
    private const double BorderWidth = 1;
    private const double InnerLineInset = 1;

    private static readonly GuiColor BorderColor = GuiColor.FromRgba(0.80, 0.70, 0.58, 0.20);
    private static readonly GuiColor TopLeftShadowColor = GuiColor.FromRgba(0, 0, 0, 0.28);
    private static readonly GuiColor BottomRightHighlightColor = GuiColor.FromRgba(1, 0.92, 0.80, 0.10);

    public GuiColor FillColor { get; set; } = GuiColor.FromRgba(0.13, 0.10, 0.07, 0.22);

    protected override void DrawBackground(Context context, GuiComponentBounds bounds)
    {
        context.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        context.FillSolid(FillColor);

        context.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        context.StrokeSolid(BorderColor, BorderWidth);

        context.EdgeLine(bounds.X + InnerLineInset, bounds.Y + InnerLineInset,
            bounds.Width - InnerLineInset * 2, bounds.Height - InnerLineInset * 2, GuiSide.Top);
        context.StrokeSolid(TopLeftShadowColor, BorderWidth);

        context.EdgeLine(bounds.X + InnerLineInset, bounds.Y + InnerLineInset,
            bounds.Width - InnerLineInset * 2, bounds.Height - InnerLineInset * 2, GuiSide.Left);
        context.StrokeSolid(TopLeftShadowColor, BorderWidth);

        context.EdgeLine(bounds.X + InnerLineInset, bounds.Y + InnerLineInset,
            bounds.Width - InnerLineInset * 2, bounds.Height - InnerLineInset * 2, GuiSide.Bottom);
        context.StrokeSolid(BottomRightHighlightColor, BorderWidth);

        context.EdgeLine(bounds.X + InnerLineInset, bounds.Y + InnerLineInset,
            bounds.Width - InnerLineInset * 2, bounds.Height - InnerLineInset * 2, GuiSide.Right);
        context.StrokeSolid(BottomRightHighlightColor, BorderWidth);
    }
}
