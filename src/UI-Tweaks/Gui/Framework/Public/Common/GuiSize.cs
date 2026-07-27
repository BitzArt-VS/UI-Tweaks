namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Width/height pair used by layout arrangement, in logical pixels.
/// A <c>null</c> dimension is unlimited along that axis.
/// </summary>
public readonly record struct GuiSize(double? Width, double? Height)
{
    public static GuiSize operator +(
        GuiSize size,
        GuiSize other)
        => new(
            Clamp(size.Width + other.Width),
            Clamp(size.Height + other.Height));

    public static GuiSize operator -(
        GuiSize size,
        GuiSize other)
        => new(
            Clamp(size.Width - other.Width),
            Clamp(size.Height - other.Height));

    public static GuiSize operator +(
        GuiSize size,
        GuiThickness thickness)
        => new(
            Clamp(size.Width + thickness.Horizontal),
            Clamp(size.Height + thickness.Vertical));

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
