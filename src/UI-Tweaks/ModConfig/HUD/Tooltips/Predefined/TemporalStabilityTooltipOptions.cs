using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public record TemporalStabilityTooltipOptions : PredefinedTooltipOptions
{
    public override string ComponentName => "ui-tweaks-tooltips-temporal-stability";
    public override string Format { get; set; } = "<font align=left>{player-temporal-stability}%</font><font align=right>({player-location-temporal-stability}%)</font>";

    public override string DialogArea { get; set; } = EnumDialogArea.CenterBottom.ToString();
    public override double Width { get; set; } = 120;
    public override bool CenterText { get; set; } = false;
    public override ComponentOffset Offset { get; set; } = new()
    {
        X = 0.0,
        Y = -60.0
    };

    public override bool HasBackground { get; set; } = true;
    public override double BackgroundOpacity { get; set; } = 0.85;
}

