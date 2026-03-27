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

public record ModHotKey
{
    public string Code { get; private init; }
    public HotkeyType HotkeyType { get; private init; }
    public string? Name { get; private init; }
    public GlKeys? DefaultKey { get; private init; }

    public bool Shift { get; private init; }
    public bool Ctrl { get; private init; }
    public bool Alt { get; private init; }

    public ModHotKey(string code, HotkeyType type, string? name = null, GlKeys? defaultKey = null, bool shift = false, bool ctrl = false, bool alt = false)
    {
        Code = code;
        HotkeyType = type;
        Name = name;
        DefaultKey = defaultKey;
        Shift = shift;
        Ctrl = ctrl;
        Alt = alt;
    }

    public static ModHotKey FromLang(string modId, string code, HotkeyType type, string? name, GlKeys? defaultKey = null, bool shift = false, bool ctrl = false, bool alt = false)
    {
        return new ModHotKey($"{modId}:{code}", type, Lang.Get($"{modId}:{name}"), defaultKey, shift, ctrl, alt);
    }
}

public static class ModHotKeyExtensions
{
    public static void AddHotKey(this IInputAPI inputApi, ModHotKey hotKey, ActionConsumable<KeyCombination> handler)
    {
        inputApi.RegisterHotKey(hotKey.Code, hotKey.Name, hotKey.DefaultKey!.Value, HotkeyType.GUIOrOtherControls, hotKey.Alt, hotKey.Ctrl, hotKey.Shift);
        inputApi.SetHotKeyHandler(hotKey.Code, handler);
    }
}