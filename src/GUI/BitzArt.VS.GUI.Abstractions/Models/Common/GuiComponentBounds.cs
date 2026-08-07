namespace BitzArt.VS;

/// <summary>
/// Describes a component's bounds and the spacing around and within it.
/// </summary>
/// <param name="Bounds">
/// <see cref="GuiBounds"/> defining the component's area, including padding
/// but excluding margin.
/// </param>
/// <param name="Margin">
/// <see cref="GuiThickness"/> defining space around the component that separates
/// it from siblings and its parent edge.
/// </param>
/// <param name="Padding">
/// <see cref="GuiThickness"/> defining space within the component between its
/// bounds and content.
/// </param>
public readonly record struct GuiComponentBounds(
    GuiBounds Bounds,
    GuiThickness Margin,
    GuiThickness Padding)
{
    /// <summary>
    /// Outer bounds formed by expanding <see cref="Bounds"/> by <see cref="Margin"/>.
    /// </summary>
    public GuiBounds MarginBounds => Bounds.Expand(Margin);

    /// <summary>
    /// Inner bounds formed by contracting <see cref="Bounds"/> by <see cref="Padding"/>.
    /// </summary>
    public GuiBounds ContentBounds => Bounds.Contract(Padding);
}
