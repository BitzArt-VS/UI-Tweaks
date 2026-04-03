using Newtonsoft.Json;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public record CustomTooltipOptions : TooltipOptions, IHudTooltipConfiguration
{
    EnumDialogArea IHudTooltipConfiguration.Area => Enum.Parse<EnumDialogArea>(DialogArea, ignoreCase: true);

    [JsonProperty("name", Order = 01)]
    public string ComponentName { get; set; } = "my-custom-tooltip";

    public override string Format { get; set; } = "Custom tooltip";
}

