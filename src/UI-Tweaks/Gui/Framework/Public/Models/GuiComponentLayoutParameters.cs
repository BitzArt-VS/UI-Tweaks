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
    /// Optional explicit width override. When <c>null</c>, the size is determined by <see cref="WidthMode"/>.
    /// Takes priority over <see cref="WidthMode"/> when set.
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
    /// Optional explicit height override. When <c>null</c>, the size is determined by <see cref="HeightMode"/>.
    /// Takes priority over <see cref="HeightMode"/> when set.
    /// </summary>
    public GuiLengthRule? Height { get; set; }

    /// <summary>
    /// Optional maximum height constraint.
    /// </summary>
    public GuiLengthRule? MaximumHeight { get; set; }

    /// <summary>
    /// How to resolve width when <see cref="Width"/> is <c>null</c>.
    /// <see cref="GuiSizeMode.Fill"/> stretches to available space.
    /// <see cref="GuiSizeMode.FitContent"/> uses measured content width plus padding.
    /// </summary>
    public GuiSizeMode WidthMode { get; set; } = GuiSizeMode.FitContent;

    /// <summary>
    /// How to resolve height when <see cref="Height"/> is <c>null</c>.
    /// <see cref="GuiSizeMode.Fill"/> stretches to available space.
    /// <see cref="GuiSizeMode.FitContent"/> uses measured content height plus padding.
    /// </summary>
    public GuiSizeMode HeightMode { get; set; } = GuiSizeMode.FitContent;

    /// <summary>
    /// Horizontal alignment of this slot within the available cross-axis space. Applies on
    /// the cross axis of relative slots whose parent stacks vertically, and on both axes of
    /// absolute slots. Has no effect when <see cref="WidthMode"/> is <see cref="GuiSizeMode.Fill"/>
    /// (no slack to align against). See <see cref="GuiHorizontalAlignment"/>.
    /// </summary>
    public GuiHorizontalAlignment HorizontalAlignment { get; set; } = GuiHorizontalAlignment.Left;

    /// <summary>
    /// Vertical alignment of this slot within the available cross-axis space. Applies on
    /// the cross axis of relative slots whose parent stacks horizontally, and on both axes
    /// of absolute slots. Has no effect when <see cref="HeightMode"/> is <see cref="GuiSizeMode.Fill"/>
    /// (no slack to align against). See <see cref="GuiVerticalAlignment"/>.
    /// </summary>
    public GuiVerticalAlignment VerticalAlignment { get; set; } = GuiVerticalAlignment.Top;

    public GuiComponentBounds ResolveBounds(
        GuiComponentBounds availableBounds)
        => ResolveBounds(availableBounds, null);

    /// <summary>
    /// Resolves component bounds from the space supplied by its parent and the
    /// inner bounds produced by its descendants.
    /// </summary>
    public GuiComponentBounds ResolveBounds(
        GuiComponentBounds availableBounds,
        GuiComponentBounds? innerContentBounds)
    {
        GuiComponentBounds adjustedAvailableBounds =
            availableBounds - Margin;
        GuiSize? availableSize = adjustedAvailableBounds.Size;
        GuiSize? innerContentSize = innerContentBounds?.Size;
        bool isProvisional = innerContentBounds is null;

        return new GuiComponentBounds(
            adjustedAvailableBounds.Position,
            new GuiSize(
                ResolveLength(
                    availableSize?.Width,
                    innerContentSize?.Width,
                    isProvisional,
                    WidthMode,
                    Padding.Horizontal,
                    Width,
                    MinimumWidth,
                    MaximumWidth),
                ResolveLength(
                    availableSize?.Height,
                    innerContentSize?.Height,
                    isProvisional,
                    HeightMode,
                    Padding.Vertical,
                    Height,
                    MinimumHeight,
                    MaximumHeight)));
    }

    private static double? ResolveLength(
        double? availableLength,
        double? innerContentLength,
        bool isProvisional,
        GuiSizeMode sizeMode,
        double padding,
        GuiLengthRule? explicitRule,
        GuiLengthRule? minimumRule,
        GuiLengthRule? maximumRule)
    {
        double? candidate = isProvisional
            ? availableLength
            : ResolveFinalCandidate(
                availableLength,
                innerContentLength,
                sizeMode,
                padding);

        return GuiLengthRule.Resolve(
            availableLength,
            candidate,
            explicitRule,
            minimumRule,
            maximumRule);
    }

    private static double? ResolveFinalCandidate(
        double? availableLength,
        double? innerContentLength,
        GuiSizeMode sizeMode,
        double padding)
    {
        double? contentLength = innerContentLength is null
            ? null
            : innerContentLength + padding;

        return sizeMode switch
        {
            GuiSizeMode.FitContent => contentLength,
            GuiSizeMode.Fill => availableLength ?? contentLength,
            _ => throw new ArgumentOutOfRangeException(
                nameof(sizeMode),
                sizeMode,
                "Unsupported GUI size mode."),
        };
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
        WidthMode = GuiSizeMode.FitContent;
        HeightMode = GuiSizeMode.FitContent;
        HorizontalAlignment = GuiHorizontalAlignment.Left;
        VerticalAlignment = GuiVerticalAlignment.Top;
    }
}
