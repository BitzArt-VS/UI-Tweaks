using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public record HungerRateTooltipOptions : PredefinedTooltipOptions
{
    public override string ComponentName => "ui-tweaks-tooltips-hunger";
    public override string Format { get; set; } = "{player-satiety-hunger}%";

    public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
    public override double Width { get; set; } = 56;

    public override ComponentOffset Offset { get; set; } = new()
    {
        X = 360.0,
        Y = -88.0
    };

    public override bool HasBackground { get; set; } = true;
}

