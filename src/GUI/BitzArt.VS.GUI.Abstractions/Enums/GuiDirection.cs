namespace BitzArt.VS;

/// <summary>
/// Specifies one or more axes.
/// </summary>
[Flags]
public enum GuiDirection
{
    /// <summary>
    /// No axes.
    /// </summary>
    None = 0,

    /// <summary>
    /// Vertical axis.
    /// </summary>
    Vertical = 1 << 0,

    /// <summary>
    /// Horizontal axis.
    /// </summary>
    Horizontal = 1 << 1,

    /// <summary>
    /// Both horizontal and vertical axes.
    /// </summary>
    Both = Vertical | Horizontal,
}
