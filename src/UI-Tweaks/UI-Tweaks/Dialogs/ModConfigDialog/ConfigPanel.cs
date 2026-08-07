using BitzArt.VS.GUI;
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

    protected override void DrawBackground(Context context, GuiBounds bounds)
    {
        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        double innerX = position.X + InnerLineInset;
        double innerY = position.Y + InnerLineInset;
        double innerWidth = width - InnerLineInset * 2;
        double innerHeight = height - InnerLineInset * 2;

        context.Rectangle(position.X, position.Y, width, height);
        context.FillSolid(FillColor);

        context.Rectangle(position.X, position.Y, width, height);
        context.StrokeSolid(BorderColor, BorderWidth);

        context.EdgeLine(innerX, innerY, innerWidth, innerHeight, GuiEdge.Top);
        context.StrokeSolid(TopLeftShadowColor, BorderWidth);

        context.EdgeLine(innerX, innerY, innerWidth, innerHeight, GuiEdge.Left);
        context.StrokeSolid(TopLeftShadowColor, BorderWidth);

        context.EdgeLine(innerX, innerY, innerWidth, innerHeight, GuiEdge.Bottom);
        context.StrokeSolid(BottomRightHighlightColor, BorderWidth);

        context.EdgeLine(innerX, innerY, innerWidth, innerHeight, GuiEdge.Right);
        context.StrokeSolid(BottomRightHighlightColor, BorderWidth);
    }
}
