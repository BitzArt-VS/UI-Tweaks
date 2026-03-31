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
            public override string ComponentName => "ui-tweaks-tooltips-healthbar";
            public override string Format { get; set; } = "{current} / {maximum} ({percent}%)";
        }

        public class SatietyTooltipOptions : TooltipOptions
        {
            public override string ComponentName => "ui-tweaks-tooltips-satiety";
            public override string Format { get; set; } = "{current} / {maximum} ({percent}%)   |   {hunger}%";
        }

        public abstract class TooltipOptions : IHudTooltipConfiguration
        {
            [JsonIgnore]
            public abstract string ComponentName { get; }

            [JsonProperty("enable", Order = 1)]
            public bool Enable { get; set; } = true;

            [JsonProperty("format", Order = 2)]
            public abstract string Format { get; set; }

            [JsonProperty("offset", Order = 3)]
            public ComponentOffset Offset { get; set; } = new();

            double IHudTooltipConfiguration.X => Offset.X;
            double IHudTooltipConfiguration.Y => Offset.Y;

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

