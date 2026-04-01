using Newtonsoft.Json;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public class CustomTooltipOptions : TooltipOptions, IHudTooltipConfiguration
        {
            bool IHudTooltipConfiguration.Enable => true;
            EnumDialogArea IHudTooltipConfiguration.Area => Enum.Parse<EnumDialogArea>(DialogArea, ignoreCase: true);

            [JsonProperty("name", Order = 01)]
            public string ComponentName { get; set; } = string.Empty;

            public override string Format { get; set; } = "😊 Custom tooltip 😊";
        }
    }
}

