namespace BitzArt.VS;

/// <summary>
/// Represents width and height in logical pixels before display scaling.
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
