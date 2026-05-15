namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A 2D point in logical (unscaled) pixel space.
/// Used in mouse event payloads to carry both dialog-relative and screen-absolute positions.
/// </summary>
public readonly record struct GuiPoint(double X, double Y);
