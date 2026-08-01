using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

public record UiTweaksModConfig
{
    [JsonProperty("zoom", Order = 1)]
    public ZoomConfig Zoom { get; set; } = new();

    [JsonProperty("tweaks", Order = 2)]
    public GameTweaksConfig GameTweaks { get; set; } = new();

    [JsonProperty("hud", Order = 3)]
    public HudConfig Hud { get; set; } = new();

    [JsonProperty("quickSearch", Order = 4)]
    public QuickSearchConfig QuickSearch { get; set; } = new();
}
