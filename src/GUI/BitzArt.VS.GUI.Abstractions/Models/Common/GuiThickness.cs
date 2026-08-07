namespace BitzArt.VS;

/// <summary>
/// Represents thickness for each side of a rectangular area.
/// </summary>
/// <param name="Top">Thickness for the top side.</param>
/// <param name="Right">Thickness for the right side.</param>
/// <param name="Bottom">Thickness for the bottom side.</param>
/// <param name="Left">Thickness for the left side.</param>
public readonly record struct GuiThickness(double Top, double Right, double Bottom, double Left)
{
    /// <summary>
    /// Represents zero thickness on all sides.
    /// </summary>
    public static readonly GuiThickness Zero = new(0, 0, 0, 0);

    /// <summary>
    /// Specifies uniform thickness for all sides.
    /// </summary>
    /// <param name="all">Thickness for each side.</param>
    public GuiThickness(double all) : this(all, all, all, all) { }

    /// <summary>
    /// Specifies vertical and horizontal thickness separately.
    /// </summary>
    /// <param name="vertical">Thickness for the top and bottom sides.</param>
    /// <param name="horizontal">Thickness for the left and right sides.</param>
    public GuiThickness(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }

    /// <summary>
    /// Combined horizontal thickness, calculated as the sum of <see cref="Left"/>
    /// and <see cref="Right"/>.
    /// </summary>
    public double Horizontal => Left + Right;

    /// <summary>
    /// Combined vertical thickness, calculated as the sum of <see cref="Top"/>
    /// and <see cref="Bottom"/>.
    /// </summary>
    public double Vertical => Top + Bottom;

    /// <summary>
    /// Adds corresponding sides of two <see cref="GuiThickness"/> values.
    /// </summary>
    /// <param name="thickness">First value to add.</param>
    /// <param name="other">Second value to add.</param>
    /// <returns>
    /// <see cref="GuiThickness"/> containing the sum for each side.
    /// </returns>
    public static GuiThickness operator +(
        GuiThickness thickness,
        GuiThickness other)
        => new(thickness.Top + other.Top,
            thickness.Right + other.Right,
            thickness.Bottom + other.Bottom,
            thickness.Left + other.Left);

    /// <summary>
    /// Subtracts corresponding sides of one <see cref="GuiThickness"/> value from another.
    /// </summary>
    /// <param name="thickness">Value to subtract from.</param>
    /// <param name="other">Value to subtract.</param>
    /// <returns>
    /// <see cref="GuiThickness"/> containing the difference for each side.
    /// </returns>
    public static GuiThickness operator -(
        GuiThickness thickness,
        GuiThickness other)
        => new(thickness.Top - other.Top,
            thickness.Right - other.Right,
            thickness.Bottom - other.Bottom,
            thickness.Left - other.Left);
}
