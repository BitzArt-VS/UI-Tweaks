using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        [JsonProperty("healthbarTooltip")]
        public TooltipOptions HealthbarTooltip { get; set; } = new();

        [JsonProperty("satietyTooltip")]
        public TooltipOptions SatietyTooltip { get; set; } = new();

        public class TooltipOptions
        {
            [JsonProperty("enable")]
            public bool Enable { get; set; } = true;

            [JsonProperty("format")]
            public string Format { get; set; } = "{0} / {1}";

            [JsonProperty("offset")]
            public ComponentOffset Offset { get; set; } = new();

            public class ComponentOffset
            {
                [JsonProperty("x")]
                public double X { get; set; } = 0.0;

                [JsonProperty("y")]
                public double Y { get; set; } = 0.0;
            }
        }
    }
}

