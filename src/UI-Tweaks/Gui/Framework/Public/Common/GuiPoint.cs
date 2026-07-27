namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Coordinates or displacement in logical GUI space, where X increases to the right
/// and Y increases downward.
/// </summary>
/// <param name="X">Horizontal coordinate or displacement.</param>
/// <param name="Y">Vertical coordinate or displacement.</param>
/// <param name="IsAbsolute">
/// Whether this value is a resolved coordinate rather than a relative displacement.
/// </param>
public readonly record struct GuiPoint(
    double X,
    double Y,
    bool IsAbsolute = false)
{
    public static GuiPoint operator +(
        GuiPoint point,
        GuiPoint other)
    {
        EnsureExactlyOneAbsolute(point, other);

        return new(
            point.X + other.X,
            point.Y + other.Y,
            IsAbsolute: true);
    }

    public static GuiPoint operator -(
        GuiPoint point,
        GuiPoint other)
    {
        EnsureExactlyOneAbsolute(point, other);

        return new(
            point.X - other.X,
            point.Y - other.Y,
            IsAbsolute: true);
    }

    public static GuiPoint operator +(
        GuiPoint point,
        GuiThickness thickness)
    {
        EnsureAbsolute(point);

        return new(
            point.X + thickness.Left,
            point.Y + thickness.Top,
            IsAbsolute: true);
    }

    public static GuiPoint operator -(
        GuiPoint point,
        GuiThickness thickness)
    {
        EnsureAbsolute(point);

        return new(
            point.X - thickness.Left,
            point.Y - thickness.Top,
            IsAbsolute: true);
    }

    private static void EnsureExactlyOneAbsolute(
        GuiPoint point,
        GuiPoint other)
    {
        if (point.IsAbsolute == other.IsAbsolute)
        {
            throw new InvalidOperationException(
                "Point arithmetic requires exactly one absolute point and one relative point.");
        }
    }

    private static void EnsureAbsolute(GuiPoint point)
    {
        if (!point.IsAbsolute)
        {
            throw new InvalidOperationException(
                "Thickness can only be applied to an absolute point.");
        }
    }
}
