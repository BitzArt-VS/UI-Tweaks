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
    GuiThickness Margin = default,
    GuiThickness Padding = default)
{
    /// <summary>
    /// Consumes the specified thickness from the top side of these bounds.
    /// </summary>
    public GuiBounds Consume(GuiThickness consumed)
        => new(Position + consumed, Size - consumed, Margin, Padding);

    /// <summary>
    /// Subtracts content horizontally after consuming this bounds' margin.
    /// </summary>
    public GuiBounds SubtractHorizontal(GuiBounds contentBounds)
    {
        var availableBounds = Consume(Margin) with { Margin = GuiThickness.Zero };

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

        var consumedWidth = (contentBounds.Size.Value.Width ?? availableBounds.Size.Value.Width ?? 0) + contentBounds.Margin.Horizontal;

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
        var availableBounds = Consume(Margin) with { Margin = GuiThickness.Zero };

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

        var consumedHeight = (contentBounds.Size.Value.Height ?? availableBounds.Size.Value.Height ?? 0) + contentBounds.Margin.Vertical;

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
