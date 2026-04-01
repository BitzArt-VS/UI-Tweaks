using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public abstract class PredefinedTooltipOptions : TooltipOptions, IHudTooltipConfiguration, IFormatReplacements
        {
            EnumDialogArea IHudTooltipConfiguration.Area => Enum.Parse<EnumDialogArea>(DialogArea, ignoreCase: true);

            [JsonIgnore]
            public abstract string ComponentName { get; }

            [JsonProperty("enable", Order = 01)]
            public bool Enable { get; set; } = true;

            [JsonIgnore]
            protected virtual IEnumerable<FormatReplacement>? FormatReplacements => null;

            string IFormatReplacements.Replace(string format)
            {
                if (FormatReplacements is null)
                {
                    return format;
                }

                foreach (var (placeholder, recordKey) in FormatReplacements)
                {
                    format = format.Replace($"{{{placeholder}}}", $"{{{recordKey}}}", StringComparison.OrdinalIgnoreCase);
                }

                return format;
            }

            protected record struct FormatReplacement(string Placeholder, string RecordKey);
        }
    }
}

