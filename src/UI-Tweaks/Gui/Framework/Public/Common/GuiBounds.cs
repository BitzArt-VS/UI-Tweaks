namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A component's size and optional position in logical GUI coordinates.
/// </summary>
/// <param name="Position">
/// The top-left reference point, or <c>null</c> when position is unresolved.
/// </param>
/// <param name="Size">
/// The arranged size, or <c>null</c> when size is unknown. A non-null size may
/// contain unlimited dimensions represented by <c>null</c> values.
/// </param>
public readonly record struct GuiBounds(
    GuiPoint? Position,
    GuiSize? Size,
    GuiThickness? Margin = null,
    GuiThickness? Padding = null)
{
    public GuiBounds ToMarginBounds(
        GuiThickness? resultMargin = null,
        GuiThickness? resultPadding = null)
    {
        GuiThickness appliedMargin =
            Margin ?? GuiThickness.Zero;

        return new GuiBounds(
            Position - appliedMargin,
            Size + appliedMargin,
            resultMargin,
            resultPadding);
    }

    public GuiBounds ToContentBounds(
        GuiThickness? resultMargin = null,
        GuiThickness? resultPadding = null)
    {
        GuiThickness appliedPadding =
            Padding ?? GuiThickness.Zero;

        return new GuiBounds(
            Position + appliedPadding,
            Size - appliedPadding,
            resultMargin,
            resultPadding);
    }

    /// <summary>
    /// Consumes the specified thickness from the top side of these bounds.
    /// </summary>
    public GuiBounds Consume(GuiThickness consumed)
        => new(Position + consumed, Size - consumed, Margin, Padding);

    /// <summary>
    /// Returns the smallest bounds containing these bounds and the specified bounds.
    /// </summary>
    public GuiBounds Union(GuiBounds other)
    {
        if (Position is not GuiPoint position
            || other.Position is not GuiPoint otherPosition)
        {
            return new(null, null);
        }

        var left = Math.Min(position.X, otherPosition.X);
        var top = Math.Min(position.Y, otherPosition.Y);

        var right =
            GetEnd(position.X, Size?.Width) is double rightEdge
            && GetEnd(otherPosition.X, other.Size?.Width) is double otherRightEdge
                ? Math.Max(rightEdge, otherRightEdge)
                : null;

        var bottom =
            GetEnd(position.Y, Size?.Height) is double bottomEdge
            && GetEnd(otherPosition.Y, other.Size?.Height) is double otherBottomEdge
                ? Math.Max(bottomEdge, otherBottomEdge)
                : null;

        return new(
            new(left, top),
            new(
                right - left,
                bottom - top));
    }

    private static double? GetEnd(
        double start,
        double? length)
        => length is double resolvedLength
            ? start + resolvedLength
            : null;

    /// <summary>
    /// Subtracts content horizontally after consuming this bounds' margin.
    /// </summary>
    public GuiBounds SubtractHorizontal(GuiBounds contentBounds)
    {
        var availableBounds =
            Consume(Margin ?? GuiThickness.Zero)
            with
            {
                Margin = GuiThickness.Zero,
            };

        if (availableBounds.Position is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved position.");
        }

        if (availableBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved size.");
        }

        if (contentBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds with an unresolved size.");
        }

        var consumedWidth =
            (contentBounds.Size.Value.Width
                ?? availableBounds.Size.Value.Width
                ?? 0)
            + (contentBounds.Margin?.Horizontal ?? 0);

        var remainingWidth = contentBounds.Size.Value.Width is null
            ? 0
            : (availableBounds.Size.Value - new GuiSize(consumedWidth, 0)).Width;

        return new(
            availableBounds.Position.Value + new GuiPoint(consumedWidth, 0),
            new GuiSize(remainingWidth, availableBounds.Size.Value.Height),
            availableBounds.Margin,
            availableBounds.Padding);
    }

    /// <summary>
    /// Subtracts content vertically after consuming this bounds' margin.
    /// </summary>
    public GuiBounds SubtractVertical(GuiBounds contentBounds)
    {
        var availableBounds =
            Consume(Margin ?? GuiThickness.Zero)
            with
            {
                Margin = GuiThickness.Zero,
            };

        if (availableBounds.Position is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved position.");
        }

        if (availableBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved size.");
        }

        if (contentBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds with an unresolved size.");
        }

        var consumedHeight =
            (contentBounds.Size.Value.Height
                ?? availableBounds.Size.Value.Height
                ?? 0)
            + (contentBounds.Margin?.Vertical ?? 0);

        var remainingHeight = contentBounds.Size.Value.Height is null
            ? 0
            : (availableBounds.Size.Value - new GuiSize(0, consumedHeight)).Height;

        return new(
            availableBounds.Position.Value + new GuiPoint(0, consumedHeight),
            new GuiSize(availableBounds.Size.Value.Width, remainingHeight),
            availableBounds.Margin,
            availableBounds.Padding);
    }
}
