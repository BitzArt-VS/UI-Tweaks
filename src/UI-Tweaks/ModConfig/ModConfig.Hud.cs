using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        [JsonProperty("healthbarTooltip")]
        public HealthbarTooltipOptions HealthbarTooltip { get; set; } = new();

        [JsonProperty("satietyTooltip")]
        public SatietyTooltipOptions SatietyTooltip { get; set; } = new();

        public class HealthbarTooltipOptions : TooltipOptions
        {
            public override string Format { get; set; } = "{current} / {maximum} ({percent}%)";
        }

        public class SatietyTooltipOptions : TooltipOptions
        {
            public override string Format { get; set; } = "{current} / {maximum} ({percent}%)   |   {hunger}%";
        }

        public class TooltipOptions
        {
            [JsonProperty("enable")]
            public bool Enable { get; set; } = true;

            [JsonProperty("format")]
            public virtual string Format { get; set; } = "{current} / {maximum}";

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

