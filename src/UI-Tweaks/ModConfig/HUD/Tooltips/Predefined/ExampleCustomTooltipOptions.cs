namespace BitzArt.UI.Tweaks.Config;

public record ExampleCustomTooltipOptions : CustomTooltipOptions
{
    public override bool Enable { get; set; } = false;

    public override string Format { get; set; } = "This is an example of a custom tooltip. " +
        "Provided for demonstration purposes only and will not be enabled in game, even if you set 'enable' property to 'true'. " +
        "To add a custom tooltip, add a tooltip configuration object just like this one to the 'customTooltips' collection " +
        "and set 'enable' to 'true' there.";
}
