using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public class TemporalStabilityTooltipOptions : PredefinedTooltipOptions
        {
            public override string ComponentName => "ui-tweaks-tooltips-temporal-stability";
            public override string Format { get; set; } = "{own}%   ({at-location}%)";

            public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
            public override double Width { get; set; } = 96;
            public override ComponentOffset Offset { get; set; } = new()
            {
                X = 0.0,
                Y = -71.0
            };

            public override bool HasBackground { get; set; } = true;
            public override double BackgroundOpacity => 0.75;

            protected override IEnumerable<FormatReplacement>? FormatReplacements =>
            [
                new("own", "player-temporal-stability"),
                new("at-location", "player-location-temporal-stability")
            ];
        }
    }
}

