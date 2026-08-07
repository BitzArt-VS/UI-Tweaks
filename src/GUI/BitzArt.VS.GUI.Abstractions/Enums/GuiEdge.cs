namespace BitzArt.VS;

/// <summary>
/// Specifies one or more edges of rectangular bounds.
/// </summary>
[Flags]
public enum GuiEdge
{
    /// <summary>
    /// No edges.
    /// </summary>
    None = 0,

    /// <summary>
    /// Top edge.
    /// </summary>
    Top = 1 << 0,

    /// <summary>
    /// Bottom edge.
    /// </summary>
    Bottom = 1 << 1,

    /// <summary>
    /// Left edge.
    /// </summary>
    Left = 1 << 2,

    /// <summary>
    /// Right edge.
    /// </summary>
    Right = 1 << 3,
}
