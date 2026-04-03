using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public interface IHudTooltipConfiguration : IComponentOffset
{
    public string ComponentName { get; }

    public bool Enable { get; }

    public string Format { get; }

    public EnumDialogArea Area { get; }

    public double Height { get; }
    public double Width { get; }
    public bool CenterText { get; }

    public bool HasBackground { get; }
    public double BackgroundOpacity { get; }
    public double BackgroundCornerRadius { get; }

    public double FontSize { get; }
}
