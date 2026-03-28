using Newtonsoft.Json;
using System.Text.Json.Serialization;

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
            [JsonProperty("enable", Order = 1)]
            public bool Enable { get; set; } = true;

            [JsonProperty("format", Order = 2)]
            public virtual string Format { get; set; } = "{current} / {maximum}";

            [JsonProperty("offset", Order = 3)]
            public ComponentOffset Offset { get; set; } = new();

            public class ComponentOffset
            {
                [JsonProperty("x", Order = 1)]
                public double X { get; set; } = 0.0;

                [JsonProperty("y", Order = 2)]
                public double Y { get; set; } = 0.0;
            }
        }
    }
}

