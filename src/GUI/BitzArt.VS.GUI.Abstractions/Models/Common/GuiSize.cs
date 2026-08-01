namespace BitzArt.VS.GUI;

/// <summary>
/// Represents width and height measured in logical GUI pixels.
/// </summary>
/// <remarks>
/// Arithmetic treats each dimension independently. A <see langword="null"/> input
/// produces a <see langword="null"/> result on the same axis. Each finite result is
/// clamped to zero when the calculated value would be negative.
/// </remarks>
/// <param name="Width">
/// Horizontal extent, or <see langword="null"/> when no finite extent is known or imposed.
/// </param>
/// <param name="Height">
/// Vertical extent, or <see langword="null"/> when no finite extent is known or imposed.
/// </param>
public readonly record struct GuiSize(double? Width, double? Height)
{
    /// <summary>
    /// Adds the corresponding dimensions of two sizes.
    /// </summary>
    /// <param name="size">First size to add.</param>
    /// <param name="other">Second size to add.</param>
    /// <returns>The combined size.</returns>
    public static GuiSize operator +(
        GuiSize size,
        GuiSize other)
        => new(
            Clamp(size.Width + other.Width),
            Clamp(size.Height + other.Height));

    /// <summary>
    /// Subtracts the corresponding dimensions of one size from another.
    /// </summary>
    /// <param name="size">Size from which to subtract.</param>
    /// <param name="other">Size to subtract.</param>
    /// <returns>The reduced size.</returns>
    public static GuiSize operator -(
        GuiSize size,
        GuiSize other)
        => new(
            Clamp(size.Width - other.Width),
            Clamp(size.Height - other.Height));

    /// <summary>
    /// Expands a size by the horizontal and vertical totals of a thickness.
    /// </summary>
    /// <param name="size">Size to expand.</param>
    /// <param name="thickness">Thickness to add.</param>
    /// <returns>The expanded size.</returns>
    public static GuiSize operator +(
        GuiSize size,
        GuiThickness thickness)
        => new(
            Clamp(size.Width + thickness.Horizontal),
            Clamp(size.Height + thickness.Vertical));

    /// <summary>
    /// Contracts a size by the horizontal and vertical totals of a thickness.
    /// </summary>
    /// <param name="size">Size to contract.</param>
    /// <param name="thickness">Thickness to subtract.</param>
    /// <returns>The contracted size.</returns>
    public static GuiSize operator -(
        GuiSize size,
        GuiThickness thickness)
        => new(
            Clamp(size.Width - thickness.Horizontal),
            Clamp(size.Height - thickness.Vertical));

    private static double? Clamp(double? value)
        => value is null
            ? null
            : Math.Max(0, value.Value);
}
