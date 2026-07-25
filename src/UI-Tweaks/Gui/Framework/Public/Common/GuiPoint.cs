namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A point in logical GUI coordinates, where X increases to the right and
/// Y increases downward.
/// </summary>
public readonly record struct GuiPoint(double X, double Y)
{
    public static GuiPoint operator +(
        GuiPoint point,
        GuiThickness thickness)
        => new(
            point.X + thickness.Left,
            point.Y + thickness.Top);

    public static GuiPoint operator -(
        GuiPoint point,
        GuiThickness thickness)
        => new(
            point.X - thickness.Left,
            point.Y - thickness.Top);
}
