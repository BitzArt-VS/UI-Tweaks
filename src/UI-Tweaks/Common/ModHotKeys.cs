using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

public static class ModHotKeys
{
    public static ModHotKey QuickSearch { get; }
        = ModHotKey.FromLang(Constants.ModId, "search", HotkeyType.HelpAndOverlays, "quicksearch", GlKeys.N);

    public static ModHotKey ModConfiguration { get; }
        = ModHotKey.FromLang(Constants.ModId, "config", HotkeyType.HelpAndOverlays, "ui-tweaks-config", GlKeys.N, shift: true);
}

public record ModHotKey(
    string Code,
    HotkeyType HotkeyType,
    string? Name = null,
    GlKeys? DefaultKey = null,
    bool Shift = false,
    bool Ctrl = false,
    bool Alt = false)
{
    public static ModHotKey FromLang(string modId, string code, HotkeyType type, string? name, GlKeys? defaultKey = null, bool shift = false, bool ctrl = false, bool alt = false)
    {
        return new ModHotKey($"{modId}:{code}", type, Lang.Get($"{modId}:{name}"), defaultKey, shift, ctrl, alt);
    }
}

public static class ModHotKeyExtensions
{
    public static void AddHotKey(this IInputAPI inputApi, ModHotKey hotKey, ActionConsumable<KeyCombination> handler)
    {
        inputApi.RegisterHotKey(hotKey.Code, hotKey.Name, hotKey.DefaultKey!.Value, hotKey.HotkeyType, hotKey.Alt, hotKey.Ctrl, hotKey.Shift);
        inputApi.SetHotKeyHandler(hotKey.Code, handler);
    }
}