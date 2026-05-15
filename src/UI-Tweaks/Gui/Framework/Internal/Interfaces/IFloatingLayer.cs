using System;

namespace BitzArt.UI.Tweaks.Gui;

internal interface IFloatingLayer : IDisposable
{
    void OnFrameStart();
    void RunWalk();
    void Render();
}
