using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public interface IHudTooltipConfiguration : IComponentOffset
{
    public string ComponentName { get; }
    public bool Enable { get; }
    public string Format { get; }
    public EnumDialogArea Area { get; }
    public double Width { get; }
    public bool HasBackground { get; }
    public double BackgroundOpacity { get; }
}
