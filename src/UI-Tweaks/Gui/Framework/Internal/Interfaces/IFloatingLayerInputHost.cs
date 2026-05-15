namespace BitzArt.UI.Tweaks.Gui;

internal interface IFloatingLayerInputHost
{
    void AddInteractiveRegion(in InteractiveRegion region);
    void AddKeyboardRegion(in KeyboardRegion region);
}
