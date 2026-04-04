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

public record ComponentPadding : IComponentPadding
{
    [JsonProperty("top", Order = 1)]
    public double Top { get; set; } = 0.0;

    [JsonProperty("right", Order = 2)]
    public double Right { get; set; } = 0.0;

    [JsonProperty("bottom", Order = 3)]
    public double Bottom { get; set; } = 0.0;

    [JsonProperty("left", Order = 4)]
    public double Left { get; set; } = 0.0;

    public ComponentPadding(double padding) : this(padding, padding) { }

    public ComponentPadding(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }

    public ComponentPadding(double top, double right, double bottom, double left) : this()
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public ComponentPadding() { }
}
