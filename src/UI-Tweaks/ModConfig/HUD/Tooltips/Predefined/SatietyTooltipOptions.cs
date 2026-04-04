using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public record SatietyTooltipOptions : TooltipOptions
{
    public override bool Enable { get; set; } = true;

    public override string ComponentName => "ui-tweaks-tooltips-satiety";
    public override string Format { get; set; } = "<font align=left>{player-satiety-current} / {player-satiety-max}</font><font align=right>({player-satiety-percent}%)</font>";

    public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
    public override double Width { get; set; } = 144;
    public override ComponentOffset Offset { get; set; } = new()
    {
        X = 250.0,
        Y = -88.0
    };

    public override bool HasBackground { get; set; } = true;
}

