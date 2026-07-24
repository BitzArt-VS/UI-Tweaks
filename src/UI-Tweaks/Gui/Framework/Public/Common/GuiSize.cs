namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Width/height pair used by layout arrangement, in logical pixels.
/// A <c>null</c> dimension is unlimited along that axis.
/// </summary>
public readonly record struct GuiSize(double? Width, double? Height);
