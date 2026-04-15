using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal abstract class ConfigPage(string title) : IDisposable
{
    protected const int NavButtonWidth = 200;
    protected const int NavButtonHeight = 28;
    protected const int NavButtonGap = 8;
    protected const int ContentTopPadding = 16;

    public string Title { get; } = title;

    protected readonly CairoFont TextFont = CairoFont.WhiteSmallText();

    public abstract double ComposeContent(ICoreClientAPI clientApi, GuiComposer composer, ElementBounds bounds, Action saveConfig, Action<ConfigPage> pushPage);

    public virtual void OnComposed(GuiComposer composer) { }

    public virtual void Dispose()
    {
        TextFont.Dispose();
        GC.SuppressFinalize(this);
    }

    protected static void AddNavButton(GuiComposer composer, string langKey, string key, double x, ref double y, Action onClick)
        => AddNavButton(composer, langKey, key, x, ref y, () => { onClick.Invoke(); return true; });

    protected static void AddNavButton(GuiComposer composer, string langKey, string key, double x, ref double y, ActionConsumable onClick)
    {
        var buttonBounds = ElementBounds.Fixed(x, y, NavButtonWidth, NavButtonHeight);
        composer.AddSmallButton(Lang.Get(langKey), onClick, buttonBounds, key: key);
        y += NavButtonHeight + NavButtonGap;
    }
}

internal static class GuiComposerConfigExtensions
{
    public static GuiComposer AddConfigHoverText(this GuiComposer composer, string translationId, CairoFont font, ElementBounds bounds, bool requiresRestart = false)
    {
        var text = Lang.Get($"{Constants.ModId}:{translationId}");

        if (requiresRestart)
        {
            text += $"\n\n<i>{Lang.Get($"{Constants.ModId}:config-requires-restart")}</i>";
        }

        return composer.AddHoverText(text, font, (int)bounds.fixedWidth, bounds);
    }
}
