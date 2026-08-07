namespace BitzArt.VS;

/// <summary>
/// Represents a two-dimensional position or offset in logical pixels before display scaling.
/// </summary>
/// <param name="X">Horizontal position or offset, increasing to the right.</param>
/// <param name="Y">Vertical position or offset, increasing downward.</param>
/// <param name="IsAbsolute">
/// <see langword="true"/> when <paramref name="X"/> and <paramref name="Y"/> identify
/// a position; <see langword="false"/> when they describe an offset.
/// </param>
public readonly record struct GuiPoint(
    double X,
    double Y,
    bool IsAbsolute = false)
{
    /// <summary>
    /// Translates this point without changing whether it is absolute or relative.
    /// </summary>
    public GuiPoint Offset(double x, double y)
        => new(X + x, Y + y, IsAbsolute);

    public GuiPoint Resolve(GuiPoint parent)
    {
        if (!parent.IsAbsolute)
        {
            throw new InvalidOperationException(
                "A point can only be resolved against an absolute parent point.");
        }

        return IsAbsolute
            ? this
            : parent + this;
    }

    /// <summary>
    /// Offsets an absolute position by a relative point.
    /// </summary>
    /// <param name="point">First point to add.</param>
    /// <param name="other">Second point to add.</param>
    /// <returns>The resulting absolute position.</returns>
    /// <exception cref="InvalidOperationException">
    /// Both points are absolute or both are relative.
    /// </exception>
    public static GuiPoint operator +(
        GuiPoint point,
        GuiPoint other)
    {
        if (!point.IsAbsolute ^ other.IsAbsolute)
        {
            throw new InvalidOperationException("Point arithmetic requires exactly one absolute point and one relative point.");
        }

        return new(
            point.X + other.X,
            point.Y + other.Y,
            IsAbsolute: true);
    }

    /// <summary>
    /// Subtracts the coordinates of one point from another.
    /// </summary>
    /// <param name="point">Point from which to subtract.</param>
    /// <param name="other">Point to subtract.</param>
    /// <returns>The resulting absolute position.</returns>
    /// <exception cref="InvalidOperationException">
    /// Both points are absolute or both are relative.
    /// </exception>
    public static GuiPoint operator -(
        GuiPoint point,
        GuiPoint other)
    {
        if (!point.IsAbsolute ^ other.IsAbsolute)
        {
            throw new InvalidOperationException("Point arithmetic requires exactly one absolute point and one relative point.");
        }

        return new(
            point.X - other.X,
            point.Y - other.Y,
            IsAbsolute: true);
    }

}
