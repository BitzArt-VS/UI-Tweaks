namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// A component's size and optional position in logical GUI coordinates.
/// </summary>
/// <param name="Position">
/// The top-left reference point, or <c>null</c> when position is unresolved.
/// </param>
/// <param name="Size">
/// Width extends rightward and height extends downward from
/// <paramref name="Position"/>.
/// </param>
public readonly record struct GuiComponentBounds(
    GuiPoint? Position,
    GuiLayoutSize Size);
