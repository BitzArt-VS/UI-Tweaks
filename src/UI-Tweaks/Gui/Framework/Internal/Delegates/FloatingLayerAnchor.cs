using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

internal delegate (double posX, double posY) FloatingLayerAnchor(
    double physicalWidth,
    double physicalHeight,
    float scale,
    ICoreClientAPI clientApi);
