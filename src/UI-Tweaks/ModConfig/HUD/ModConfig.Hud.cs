using Newtonsoft.Json;
using System.Collections.Generic;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        [JsonProperty("healthbarTooltip")]
        public HealthbarTooltipOptions HealthbarTooltip { get; set; } = new();

        [JsonProperty("satietyTooltip")]
        public SatietyTooltipOptions SatietyTooltip { get; set; } = new();

        [JsonProperty("hungerTooltip")]
        public HungerTooltipOptions HungerTooltip { get; set; } = new();

        [JsonProperty("temporalStabilityTooltip")]
        public TemporalStabilityTooltipOptions TemporalStabilityTooltip { get; set; } = new();

        [JsonProperty("customTooltips")]
        public List<CustomTooltipOptions> CustomTooltips { get; set; } = [];
    }
}

