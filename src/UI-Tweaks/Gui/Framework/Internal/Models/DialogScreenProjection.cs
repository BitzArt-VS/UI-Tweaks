using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks.Gui;

internal sealed class DialogScreenProjection
{
    private readonly ICoreClientAPI _clientApi;
    private readonly IGuiDialog _dialog;

    internal DialogScreenProjection(ICoreClientAPI clientApi, IGuiDialog dialog)
    {
        _clientApi = clientApi;
        _dialog = dialog;
    }

    internal bool TryToLogical(int x, int y, out double logicalX, out double logicalY)
    {
        var (positionX, positionY, physicalWidth, physicalHeight, scale) = ResolveScreenRect();
        logicalX = (x - positionX) / scale;
        logicalY = (y - positionY) / scale;
        return x >= positionX && x < positionX + physicalWidth && y >= positionY && y < positionY + physicalHeight;
    }

    internal bool Contains(int x, int y)
    {
        var (positionX, positionY, physicalWidth, physicalHeight, _) = ResolveScreenRect();
        return x >= positionX && x < positionX + physicalWidth && y >= positionY && y < positionY + physicalHeight;
    }

    internal (int positionX, int positionY) GetScreenOrigin()
    {
        var (positionX, positionY, _, _, _) = ResolveScreenRect();
        return (positionX, positionY);
    }

    private (int positionX, int positionY, double physicalWidth, double physicalHeight, float scale) ResolveScreenRect()
    {
        float scale = RuntimeEnv.GUIScale;
        double physicalWidth = Math.Round(_dialog.LayoutParameters.Width.Value * scale);
        double physicalHeight = Math.Round(_dialog.LayoutParameters.Height.Value * scale);
        var (positionX, positionY) = ComputeScreenOrigin(physicalWidth, physicalHeight, scale);
        return (positionX, positionY, physicalWidth, physicalHeight, scale);
    }

    private (int positionX, int positionY) ComputeScreenOrigin(double physicalWidth, double physicalHeight, float scale)
    {
        // The surface renderer rounds logical size to physical pixels before blitting.
        // Center against that same rounded rectangle so resize anchoring cannot alternate
        // between adjacent integer origins while the logical size crosses half-pixels.
        int positionX = (int)((_clientApi.Render.FrameWidth - physicalWidth) / 2.0 + _dialog.OffsetX * scale);
        int positionY = (int)((_clientApi.Render.FrameHeight - physicalHeight) / 2.0 + _dialog.OffsetY * scale);
        return (positionX, positionY);
    }
}
