namespace BitzArt.VS.GUI;

internal readonly struct ResizeRegion
{
    public readonly GuiBounds Bounds;
    public readonly GuiBounds? ClipBounds;
    public readonly object Token;
    public readonly IGuiResizable Target;

    public ResizeRegion(GuiBounds bounds, object token, IGuiResizable target, GuiBounds? clipBounds = null)
    {
        Bounds = bounds;
        ClipBounds = clipBounds;
        Token = token;
        Target = target;
    }

    public bool Contains(double x, double y)
    {
        var point =
            new GuiPoint(
                x,
                y,
                IsAbsolute: true);

        return Bounds.Contains(point)
            && (ClipBounds is not GuiBounds clipBounds
                || clipBounds.Contains(point));
    }
}
