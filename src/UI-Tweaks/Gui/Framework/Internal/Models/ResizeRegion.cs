namespace BitzArt.UI.Tweaks.Gui;

internal readonly struct ResizeRegion
{
    public readonly GuiBounds Bounds;
    public readonly object Token;
    public readonly IGuiResizable Target;

    public ResizeRegion(GuiBounds bounds, object token, IGuiResizable target)
    {
        Bounds = bounds;
        Token = token;
        Target = target;
    }

    public bool Contains(double x, double y)
    {
        var position = Bounds.Position!.Value;
        var size = Bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        return x >= position.X && x < position.X + width
            && y >= position.Y && y < position.Y + height;
    }
}
