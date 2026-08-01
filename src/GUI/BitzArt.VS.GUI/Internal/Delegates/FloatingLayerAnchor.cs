using Vintagestory.API.Client;

namespace BitzArt.VS.GUI;

internal delegate (double posX, double posY) FloatingLayerAnchor(
    double physicalWidth,
    double physicalHeight,
    float scale,
    ICoreClientAPI clientApi);
