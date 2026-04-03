using Newtonsoft.Json;

namespace BitzArt.UI.Tweaks.Config;

public record ComponentOffset : IComponentOffset
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
