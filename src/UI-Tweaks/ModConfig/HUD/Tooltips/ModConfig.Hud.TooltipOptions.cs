using Newtonsoft.Json;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal partial class UiTweaksModConfig
{
    internal partial class HudConfig
    {
        public abstract class TooltipOptions : IComponentOffset
        {
            [JsonProperty("format", Order = 11)]
            public abstract string Format { get; set; }

            [JsonProperty("dialogArea", Order = 21)]
            public virtual string DialogArea { get; set; } = EnumDialogArea.CenterMiddle.ToString();

            [JsonProperty("width", Order = 22)]
            public virtual double Width { get; set; } = 200;

            [JsonProperty("offset", Order = 23)]
            public virtual ComponentOffset Offset { get; set; } = new();

            [JsonProperty("hasBackground", Order = 31)]
            public virtual bool HasBackground { get; set; } = false;

            [JsonProperty("backgroundOpacity", Order = 32)]
            public virtual double BackgroundOpacity => 0.6;

            double IComponentOffset.X => Offset.X;
            double IComponentOffset.Y => Offset.Y;

            public class ComponentOffset : IComponentOffset
            {
                [JsonProperty("x", Order = 1)]
                public double X { get; set; } = 0.0;

                [JsonProperty("y", Order = 2)]
                public double Y { get; set; } = 0.0;

                public ComponentOffset(double x, double y) : this()
                {
                    X = x;
                    Y = y;
                }

                public ComponentOffset() { }
            }
        }
    }
}

