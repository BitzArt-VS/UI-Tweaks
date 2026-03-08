using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace BitzArt.QuickSearch;

public static class ModHotKeys
{
    public static ModHotKey QuickSearch { get; } = ModHotKey.FromLang(Constants.ModId, "search", "hotKey_name", GlKeys.N);
}

public record ModHotKey
{
    public string Code { get; private init; }
    public string? Name { get; private init; }
    public GlKeys? DefaultKey { get; private init; }

    public ModHotKey(string code, string? name = null, GlKeys? defaultKey = null)
    {
        Code = code;
        Name = name;
        DefaultKey = defaultKey;
    }

    public static ModHotKey FromLang(string modId, string code, string? name, GlKeys? defaultKey = null)
    {
        return new ModHotKey($"{modId}:{code}", Lang.Get($"{modId}:{name}"), defaultKey);
    }
}

public static class ModHotKeyExtensions
{
    public static void AddHotKey(this IInputAPI inputApi, ModHotKey hotKey, ActionConsumable<KeyCombination> handler)
    {
        inputApi.RegisterHotKey(hotKey.Code, hotKey.Name, hotKey.DefaultKey!.Value, HotkeyType.GUIOrOtherControls);
        inputApi.SetHotKeyHandler(hotKey.Code, handler);
    }
}