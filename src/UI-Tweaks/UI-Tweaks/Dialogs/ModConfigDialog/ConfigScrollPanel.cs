using BitzArt.VS.GUI;

namespace BitzArt.UI.Tweaks;

internal sealed class ConfigScrollPanel : GuiContainer
{
    public ConfigScrollPanel()
    {
        Scroll = GuiDirection.Vertical;
        Scrollbar = GuiDirection.Vertical;
    }
}
