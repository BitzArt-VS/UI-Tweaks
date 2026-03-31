namespace BitzArt.UI.Tweaks;

public interface IHudTooltipConfiguration
{
    public string ComponentName { get; }
    public bool Enable { get; }
    public string Format { get; }
    public double X { get; }
    public double Y { get; }
}

