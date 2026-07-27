namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A component's size and optional position in logical GUI coordinates.
/// </summary>
/// <param name="Position">
/// Top-left position, or <c>null</c> when unresolved. A relative point is a transient
/// arrangement result measured from bounds supplied by the parent. An absolute point
/// is a resolved coordinate suitable for cached layout, rendering, and input.
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
    internal GuiBounds ResolveRelativePosition(
        GuiPoint absoluteOrigin)
    {
        if (Position is null || Position.Value.IsAbsolute)
        {
            return this;
        }

        if (!absoluteOrigin.IsAbsolute)
        {
            throw new InvalidOperationException(
                "A relative position requires an absolute origin.");
        }

        return this with
        {
            Position = absoluteOrigin + Position.Value,
        };
    }

    internal GuiBounds MakePositionRelative(
        GuiPoint absoluteOrigin)
    {
        if (Position is null)
        {
            return this;
        }

        if (!Position.Value.IsAbsolute || !absoluteOrigin.IsAbsolute)
        {
            throw new InvalidOperationException(
                "Only an absolute position can be made relative to an absolute origin.");
        }

        return this with
        {
            Position = new GuiPoint(
                Position.Value.X - absoluteOrigin.X,
                Position.Value.Y - absoluteOrigin.Y),
        };
    }

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
    /// Whether <paramref name="point"/> lies within these bounds. Left and top edges are
    /// inclusive; right and bottom edges are exclusive. Unresolved bounds contain no points.
    /// </summary>
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
            new(left, top, IsAbsolute: true),
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

        var contentMarginBounds =
            contentBounds.ToMarginBounds();

        var consumedWidth =
            contentMarginBounds.Position!.Value.X
            - availableBounds.Position.Value.X
            + (contentMarginBounds.Size!.Value.Width
                ?? availableBounds.Size.Value.Width
                ?? 0);

        var remainingWidth = contentMarginBounds.Size.Value.Width is null
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

        var contentMarginBounds =
            contentBounds.ToMarginBounds();

        var consumedHeight =
            contentMarginBounds.Position!.Value.Y
            - availableBounds.Position.Value.Y
            + (contentMarginBounds.Size!.Value.Height
                ?? availableBounds.Size.Value.Height
                ?? 0);

        var remainingHeight = contentMarginBounds.Size.Value.Height is null
            ? 0
            : (availableBounds.Size.Value - new GuiSize(0, consumedHeight)).Height;

        return new(
            availableBounds.Position.Value + new GuiPoint(0, consumedHeight),
            new GuiSize(availableBounds.Size.Value.Width, remainingHeight),
            availableBounds.Margin,
            availableBounds.Padding);
    }
}
