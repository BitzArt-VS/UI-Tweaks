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
    /// Consumes a vertical length from the top side of these bounds.
    /// </summary>
    public GuiBounds Consume(GuiThickness consumed)
    {
        return new GuiBounds(
            Position + consumed,
            Size - consumed,
            Margin,
            Padding);
    }
}
