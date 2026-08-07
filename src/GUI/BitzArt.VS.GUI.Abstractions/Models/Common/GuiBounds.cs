namespace BitzArt.VS;

/// <summary>
/// Describes the position and size of a rectangular area in the user interface.
/// </summary>
/// <param name="Position">
/// <see cref="GuiPoint"/> defining the position of the area's upper-left corner,
/// or <see langword="null"/> when the area has not been placed.
/// </param>
/// <param name="Size">
/// <see cref="GuiSize"/> defining the area's width and height,
/// or <see langword="null"/> when the area has not been sized.
/// </param>
public readonly record struct GuiBounds(
    GuiPoint? Position,
    GuiSize? Size)
{
    /// <summary>
    /// Extends each edge outward by its specified thickness.
    /// </summary>
    /// <param name="thickness">
    /// <see cref="GuiThickness"/> applied to the edges.
    /// </param>
    /// <returns>Expanded bounds.</returns>
    public GuiBounds Expand(GuiThickness thickness)
    {
        var position = Position?.Offset(-thickness.Left, -thickness.Top);
        return new GuiBounds(position, Size + thickness);
    }

    /// <summary>
    /// Moves each edge inward by its specified thickness.
    /// </summary>
    /// <param name="thickness">
    /// <see cref="GuiThickness"/> applied to the edges.
    /// </param>
    /// <returns>Contracted bounds with dimensions clamped to zero.</returns>
    public GuiBounds Contract(GuiThickness thickness)
    {
        var position = Position?.Offset(thickness.Left, thickness.Top);
        return new GuiBounds(position, Size - thickness);
    }

    /// <summary>
    /// Tests whether a point lies within the area.
    /// </summary>
    /// <param name="point">
    /// <see cref="GuiPoint"/> to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the point lies within bounds that have a position,
    /// width, and height; otherwise <see langword="false"/>. Top and left edges are
    /// included; bottom and right edges are excluded.
    /// </returns>
    public bool Contains(GuiPoint point)
    {
        if (Position is not GuiPoint position
            || Size?.Width is not double width
            || Size?.Height is not double height)
        {
            return false;
        }

        return point.X >= position.X
            && point.X < position.X + width
            && point.Y >= position.Y
            && point.Y < position.Y + height;
    }

    /// <summary>
    /// Returns the smallest rectangle containing both bounds, including any gap between them.
    /// </summary>
    /// <param name="other">
    /// <see cref="GuiBounds"/> to include.
    /// </param>
    /// <returns>
    /// Enclosing bounds with the same position type as the inputs. Missing either
    /// position leaves the result without a position or size. A missing dimension
    /// remains missing.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// One position is absolute and the other is relative.
    /// </exception>
    public GuiBounds Union(GuiBounds other)
    {
        if (Position is not GuiPoint position || other.Position is not GuiPoint otherPosition)
        {
            return new(null, null);
        }

        if (position.IsAbsolute != otherPosition.IsAbsolute)
        {
            throw new InvalidOperationException(
                "Bounds union requires both positions to be absolute or both to be relative.");
        }

        var left = Math.Min(position.X, otherPosition.X);
        var top = Math.Min(position.Y, otherPosition.Y);

        double? right =
            GetEnd(position.X, Size?.Width) is double rightEdge
            && GetEnd(otherPosition.X, other.Size?.Width) is double otherRightEdge
                ? Math.Max(rightEdge, otherRightEdge)
                : null;

        double? bottom =
            GetEnd(position.Y, Size?.Height) is double bottomEdge
            && GetEnd(otherPosition.Y, other.Size?.Height) is double otherBottomEdge
                ? Math.Max(bottomEdge, otherBottomEdge)
                : null;

        return new(
            new(left, top, position.IsAbsolute),
            new(right - left, bottom - top));
    }

    private static double? GetEnd(
        double start,
        double? length)
        => length is double resolvedLength
            ? start + resolvedLength
            : null;

}
