namespace BitzArt.VS.GUI;

public readonly record struct GuiComponentBounds(
    GuiBounds Bounds,
    GuiThickness Margin,
    GuiThickness Padding)
{
    public GuiBounds MarginBounds => Bounds.Expand(Margin);

    public GuiBounds ContentBounds => Bounds.Contract(Padding);
}
