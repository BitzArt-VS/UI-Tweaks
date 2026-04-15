using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

public record HudConfig
{
    [JsonProperty("tooltips", Order = 01)]
    public TooltipsConfig Tooltips { get; set; } = new();
}

