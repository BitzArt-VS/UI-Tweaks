using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

internal record UiTweaksModConfig
{
    [JsonProperty("hud", Order = 1)]
    public HudConfig Hud { get; set; } = new();

    [JsonProperty("quickSearch", Order = 2)]
    public QuickSearchConfig QuickSearch { get; set; } = new();
}
