using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public class SatietyTooltipOptions : PredefinedTooltipOptions
        {
            public override string ComponentName => "ui-tweaks-tooltips-satiety";
            public override string Format { get; set; } = "{current} / {maximum} ({percent}%)";

            public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
            public override double Width { get; set; } = 124;
            public override ComponentOffset Offset { get; set; } = new()
            {
                X = 250.0,
                Y = -87.0
            };

            public override bool HasBackground { get; set; } = true;

            protected override IEnumerable<FormatReplacement>? FormatReplacements =>
            [
                new("current", "player-satiety-current"),
                new("maximum", "player-satiety-max"),
                new("percent", "player-satiety-percent")
            ];
        }
    }
}

