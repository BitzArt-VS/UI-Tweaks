namespace BitzArt.VS.GUI;

public readonly record struct GuiBounds(
    GuiPoint? Position,
    GuiSize? Size)
{
    public GuiBounds Expand(GuiThickness thickness)
    {
        var position = Position?.Offset(-thickness.Left, -thickness.Top);
        return new GuiBounds(position, Size + thickness);
    }

    public GuiBounds Contract(GuiThickness thickness)
    {
        var position = Position?.Offset(thickness.Left, thickness.Top);
        return new GuiBounds(position, Size - thickness);
    }

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

    public GuiBounds SubtractHorizontal(GuiBounds consumedBounds)
    {
        if (Position is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved position.");
        }

        if (Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved size.");
        }

        if (consumedBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds with an unresolved size.");
        }

        var consumedWidth =
            consumedBounds.Position!.Value.X
            - Position.Value.X
            + (consumedBounds.Size.Value.Width
                ?? Size.Value.Width
                ?? 0);

        var remainingWidth = consumedBounds.Size.Value.Width is null
            ? 0
            : (Size.Value - new GuiSize(consumedWidth, 0)).Width;

        return new(
            Position.Value + new GuiPoint(consumedWidth, 0),
            new GuiSize(remainingWidth, Size.Value.Height));
    }

    public GuiBounds SubtractVertical(GuiBounds consumedBounds)
    {
        if (Position is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved position.");
        }

        if (Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds from an unresolved size.");
        }

        if (consumedBounds.Size is null)
        {
            throw new InvalidOperationException("Cannot subtract bounds with an unresolved size.");
        }

        var consumedHeight =
            consumedBounds.Position!.Value.Y
            - Position.Value.Y
            + (consumedBounds.Size.Value.Height
                ?? Size.Value.Height
                ?? 0);

        var remainingHeight = consumedBounds.Size.Value.Height is null
            ? 0
            : (Size.Value - new GuiSize(0, consumedHeight)).Height;

        return new(
            Position.Value + new GuiPoint(0, consumedHeight),
            new GuiSize(Size.Value.Width, remainingHeight));
    }
}
