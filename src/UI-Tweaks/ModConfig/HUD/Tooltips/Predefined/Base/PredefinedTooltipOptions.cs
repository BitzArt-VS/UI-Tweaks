using Newtonsoft.Json;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public abstract record PredefinedTooltipOptions : TooltipOptions, IHudTooltipConfiguration
{
    public override bool Enable { get; set; } = false;

    EnumDialogArea IHudTooltipConfiguration.Area => Enum.Parse<EnumDialogArea>(DialogArea, ignoreCase: true);

    [JsonIgnore]
    public abstract string ComponentName { get; }
}

