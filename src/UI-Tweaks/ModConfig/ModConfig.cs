using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    [JsonProperty("hud", Order = 1)]
    public HudConfig Hud { get; set; } = new();

    [JsonProperty("quickSearch", Order = 2)]
    public QuickSearchConfig QuickSearch { get; set; } = new();
}
