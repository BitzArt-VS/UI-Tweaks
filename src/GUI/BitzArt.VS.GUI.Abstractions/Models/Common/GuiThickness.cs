namespace BitzArt.VS;

public readonly record struct GuiThickness(double Top, double Right, double Bottom, double Left)
{
    public static readonly GuiThickness Zero = new(0, 0, 0, 0);

    public GuiThickness(double all) : this(all, all, all, all) { }

    public GuiThickness(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }

    public double Horizontal => Left + Right;

    public double Vertical => Top + Bottom;

    public static GuiThickness operator +(
        GuiThickness thickness,
        GuiThickness other)
        => new(thickness.Top + other.Top,
            thickness.Right + other.Right,
            thickness.Bottom + other.Bottom,
            thickness.Left + other.Left);

    public static GuiThickness operator -(
        GuiThickness thickness,
        GuiThickness other)
        => new(thickness.Top - other.Top,
            thickness.Right - other.Right,
            thickness.Bottom - other.Bottom,
            thickness.Left - other.Left);
}
