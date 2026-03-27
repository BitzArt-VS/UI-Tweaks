using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    public class QuickSearchConfig
    {
        [JsonProperty("enable")]
        public bool Enable { get; set; } = true;

        [JsonProperty("resultListHeight")]
        public int ResultListHeight { get; set; } = 200;
    }
}

