using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    [JsonProperty("hud")]
    public HudConfig Hud { get; set; } = new();

    [JsonProperty("quickSearch")]
    public QuickSearchConfig QuickSearch { get; set; } = new();
}
