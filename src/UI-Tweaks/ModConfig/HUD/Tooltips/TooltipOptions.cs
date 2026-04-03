using Newtonsoft.Json;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Config;

public abstract record TooltipOptions : IComponentOffset
{
    [JsonProperty("enable", Order = 01)]
    public virtual bool Enable { get; set; } = true;

    [JsonProperty("dialogArea", Order = 21)]
    public virtual string DialogArea { get; set; } = EnumDialogArea.CenterMiddle.ToString();

    [JsonProperty("height", Order = 22)]
    public virtual double Height { get; set; } = 25;

    [JsonProperty("width", Order = 23)]
    public virtual double Width { get; set; } = 200;

    [JsonProperty("centerText", Order = 24)]
    public virtual bool CenterText { get; set; } = true;

    [JsonProperty("offset", Order = 24)]
    public virtual ComponentOffset Offset { get; set; } = new();

    [JsonProperty("hasBackground", Order = 31)]
    public virtual bool HasBackground { get; set; } = false;

    [JsonProperty("backgroundOpacity", Order = 32)]
    public virtual double BackgroundOpacity { get; set; } = 0.5;

    [JsonProperty("backgroundCornerRadius", Order = 33)]
    public virtual double BackgroundCornerRadius { get; set; } = 12.0;

    [JsonProperty("format", Order = 41)]
    public virtual string Format { get; set; } = string.Empty;

    [JsonProperty("fontSize", Order = 51)]
    public virtual double FontSize { get; set; } = GuiStyle.DetailFontSize;

    double IComponentOffset.X => Offset.X;
    double IComponentOffset.Y => Offset.Y;
}
