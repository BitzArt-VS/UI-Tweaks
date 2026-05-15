using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks;

internal sealed class ConfigScrollPanel : GuiContainer
{
    public ConfigScrollPanel()
    {
        Scroll = GuiScrollDirection.Vertical;
        Scrollbar = GuiScrollDirection.Vertical;
    }
}
