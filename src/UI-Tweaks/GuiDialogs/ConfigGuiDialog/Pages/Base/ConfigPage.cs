using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal abstract class ConfigPage(string title) : IDisposable
{
    public string Title { get; } = title;

    public abstract void ComposeContent(ICoreClientAPI clientApi, GuiComposer composer, ElementBounds bounds, Action saveConfig, Action<ConfigPage> pushPage);

    public virtual void OnComposed(GuiComposer composer) { }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
