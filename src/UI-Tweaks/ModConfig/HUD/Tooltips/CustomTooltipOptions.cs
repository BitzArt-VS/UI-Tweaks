using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

public record CustomTooltipOptions : TooltipOptions, IHudTooltipConfiguration
{
    [JsonProperty("name", Order = 01)]
    public virtual string Name { get; set; } = "my-custom-tooltip";

    public override string ComponentName => Name;

    public override string Format { get; set; } = "My custom tooltip";
}

