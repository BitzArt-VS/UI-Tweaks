namespace BitzArt.VS.GUI;

/// <summary>
/// Specifies where a component aligns within its available space.
/// </summary>
public enum GuiAlignment
{
    /// <summary>
    /// Aligns the component with the start of the available space.
    /// </summary>
    /// <remarks>
    /// For horizontal alignment, the start is the left edge. For vertical alignment,
    /// it is the top edge.
    /// </remarks>
    Start = 0,

    /// <summary>
    /// Aligns the component with the center of the available space.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Aligns the component with the end of the available space.
    /// </summary>
    /// <remarks>
    /// For horizontal alignment, the end is the right edge. For vertical alignment,
    /// it is the bottom edge.
    /// </remarks>
    End = 2,
}
