using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

public record QuickSearchConfig
{
    [JsonProperty("enable", Order = 1)]
    public bool Enable { get; set; } = true;

    [JsonProperty("resultListHeight", Order = 2)]
    public int ResultListHeight { get; set; } = 200;
}

