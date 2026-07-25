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
public readonly record struct GuiComponentBounds(
    GuiPoint? Position,
    GuiSize? Size)
{
    public static GuiComponentBounds operator +(
        GuiComponentBounds bounds,
        GuiThickness thickness)
        => bounds.Inflate(thickness);

    public static GuiComponentBounds operator -(
        GuiComponentBounds bounds,
        GuiThickness thickness)
        => bounds.Deflate(thickness);

    /// <summary>
    /// Moves the bounds inward by the specified thickness.
    /// </summary>
    public GuiComponentBounds Deflate(GuiThickness thickness)
    {
        var adjustedPosition = Position is not null
            ? Position + thickness
            : null;

        var adjustedSize = Size is not null
            ? Size - thickness
            : null;

        return new GuiComponentBounds(
            adjustedPosition,
            adjustedSize);
    }

    /// <summary>
    /// Moves the bounds outward by the specified thickness.
    /// </summary>
    public GuiComponentBounds Inflate(GuiThickness thickness)
    {
        var adjustedPosition = Position is not null
            ? Position - thickness
            : null;

        var adjustedSize = Size is not null
            ? Size + thickness
            : null;

        return new GuiComponentBounds(
            adjustedPosition,
            adjustedSize);
    }
}
