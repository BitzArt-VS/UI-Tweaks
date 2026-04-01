using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public class HungerTooltipOptions : PredefinedTooltipOptions
        {
            public override string ComponentName => "ui-tweaks-tooltips-hunger";
            public override string Format { get; set; } = "{hunger}%";

            public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
            public override double Width { get; set; } = 48;
            public override ComponentOffset Offset { get; set; } = new()
            {
                X = 340.0,
                Y = -87.0
            };

            public override bool HasBackground { get; set; } = true;

            protected override IEnumerable<FormatReplacement>? FormatReplacements =>
            [
                new("hunger", "player-satiety-hunger")
            ];
        }
    }
}

