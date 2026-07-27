namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Carries the layout and spacing configuration for a single slot in the render tree.
/// Created by <see cref="GuiRenderTreeBuilder"/> when a component is declared, then mutated
/// in place through the fluent <see cref="IGuiTreeBuilder"/> API; consumed by the layout pass.
/// </summary>
public sealed class GuiComponentLayoutParameters
{
    public GuiComponentPositioning Positioning { get; set; } = GuiComponentPositioning.Relative;

    /// <summary>Space outside the component's border, separating it from siblings and the parent edge.</summary>
    public GuiThickness Margin { get; set; } = GuiThickness.Zero;

    /// <summary>Space between the component's border and its content / children.</summary>
    public GuiThickness Padding { get; set; } = GuiThickness.Zero;

    /// <summary>
    /// Optional minimum width constraint.
    /// </summary>
    public GuiLengthRule? MinimumWidth { get; set; }

    /// <summary>
    /// Explicit width, or <see langword="null"/> to fit descendant margin bounds plus
    /// horizontal <see cref="Padding"/>.
    /// </summary>
    public GuiLengthRule? Width { get; set; }

    /// <summary>
    /// Optional maximum width constraint.
    /// </summary>
    public GuiLengthRule? MaximumWidth { get; set; }

    /// <summary>
    /// Optional minimum height constraint.
    /// </summary>
    public GuiLengthRule? MinimumHeight { get; set; }

    /// <summary>
    /// Explicit height, or <see langword="null"/> to fit descendant margin bounds plus
    /// vertical <see cref="Padding"/>.
    /// </summary>
    public GuiLengthRule? Height { get; set; }

    /// <summary>
    /// Optional maximum height constraint.
    /// </summary>
    public GuiLengthRule? MaximumHeight { get; set; }

    /// <summary>
    /// Horizontal alignment of this slot within the available cross-axis space. Applies on
    /// the cross axis of relative slots whose parent stacks vertically, and on both axes of
    /// absolute slots. Has no effect when <see cref="Width"/> consumes all available width.
    /// See <see cref="GuiHorizontalAlignment"/>.
    /// </summary>
    public GuiHorizontalAlignment HorizontalAlignment { get; set; } = GuiHorizontalAlignment.Left;

    /// <summary>
    /// Vertical alignment of this slot within the available cross-axis space. Applies on
    /// the cross axis of relative slots whose parent stacks horizontally, and on both axes
    /// of absolute slots. Has no effect when <see cref="Height"/> consumes all available height.
    /// See <see cref="GuiVerticalAlignment"/>.
    /// </summary>
    public GuiVerticalAlignment VerticalAlignment { get; set; } = GuiVerticalAlignment.Top;

    /// <summary>
    /// Resolves provisional component bounds used to measure descendants.
    /// </summary>
    public GuiBounds ResolveBounds(
        GuiBounds availableBounds)
        => ResolveBounds(
            availableBounds,
            innerContentBounds: null,
            isContentMeasured: false);

    /// <summary>
    /// Resolves final component bounds from measured descendant margin bounds.
    /// </summary>
    /// <param name="innerContentBounds">
    /// Measured descendant margin bounds, or <see langword="null"/> for empty content.
    /// </param>
    public GuiBounds ResolveBounds(
        GuiBounds availableBounds,
        GuiBounds? innerContentBounds)
        => ResolveBounds(
            availableBounds,
            innerContentBounds,
            isContentMeasured: true);

    private GuiBounds ResolveBounds(
        GuiBounds availableBounds,
        GuiBounds? innerContentBounds,
        bool isContentMeasured)
    {
        GuiBounds adjustedAvailableBounds =
            availableBounds.Consume(Margin);

        GuiSize? availableSize =
            adjustedAvailableBounds.Size;

        GuiSize? contentSize = isContentMeasured
            ? (innerContentBounds?.Size ?? new GuiSize(0, 0)) + Padding
            : null;

        return new GuiBounds(
            adjustedAvailableBounds.Position,
            new GuiSize(
                ResolveLength(
                    availableSize?.Width,
                    contentSize?.Width,
                    isContentMeasured,
                    Width,
                    MinimumWidth,
                    MaximumWidth),
                ResolveLength(
                    availableSize?.Height,
                    contentSize?.Height,
                    isContentMeasured,
                    Height,
                    MinimumHeight,
                    MaximumHeight)),
            Margin,
            Padding);
    }

    private static double? ResolveLength(
        double? availableLength,
        double? contentLength,
        bool isContentMeasured,
        GuiLengthRule? explicitRule,
        GuiLengthRule? minimumRule,
        GuiLengthRule? maximumRule)
    {
        double? candidate = isContentMeasured
            ? contentLength
            : availableLength;

        return GuiLengthRule.Resolve(
            availableLength,
            candidate,
            explicitRule,
            minimumRule,
            maximumRule);
    }

    /// <summary>
    /// Resets all properties to their documented defaults. Called by the reconciler on
    /// reused component slots before applying the current pass's configuration actions so
    /// that each blueprint pass declares a full, fresh state rather than a delta on top of
    /// the previous pass.
    /// </summary>
    internal void Reset()
    {
        Positioning = GuiComponentPositioning.Relative;
        Margin = GuiThickness.Zero;
        Padding = GuiThickness.Zero;
        MinimumWidth = null;
        Width = null;
        MaximumWidth = null;
        MinimumHeight = null;
        Height = null;
        MaximumHeight = null;
        HorizontalAlignment = GuiHorizontalAlignment.Left;
        VerticalAlignment = GuiVerticalAlignment.Top;
    }
}
