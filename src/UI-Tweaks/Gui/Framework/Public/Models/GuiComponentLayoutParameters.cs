namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Carries placement, sizing, and spacing configuration for a <see cref="GuiComponent"/>.
/// The component owns this mutable state; tree declarations configure it through
/// <see cref="IGuiTreeBuilder"/>, and arrangement consumes it.
/// </summary>
public sealed class GuiComponentLayoutParameters
{
    /// <summary>
    /// Optional component placement. A relative point offsets the aligned position and
    /// participates in sibling flow. An absolute point is the resolved border position and
    /// does not participate in sibling flow. A <see langword="null"/> value preserves the
    /// aligned position and participates in flow.
    /// </summary>
    public GuiPoint? Position { get; set; }

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
    /// Horizontal alignment within the available width. Has no effect when
    /// <see cref="Width"/> consumes all available width.
    /// </summary>
    public GuiAlignment HorizontalAlignment { get; set; } = GuiAlignment.Start;

    /// <summary>
    /// Vertical alignment within the available height. Has no effect when
    /// <see cref="Height"/> consumes all available height.
    /// </summary>
    public GuiAlignment VerticalAlignment { get; set; } = GuiAlignment.Start;

    /// <summary>
    /// Resolves provisional component bounds used to arrange descendants.
    /// </summary>
    public GuiBounds ResolveBounds(
        GuiBounds availableBounds)
        => ResolveBounds(
            availableBounds,
            candidateSize: null,
            useAvailableSize: true);

    internal GuiBounds ResolveProvisionalBounds(
        GuiBounds availableBounds,
        GuiBounds? previousBounds)
        => previousBounds is null
            ? ResolveBounds(availableBounds)
            : ResolveBounds(
                availableBounds,
                previousBounds.Value.Size,
                useAvailableSize: false);

    /// <summary>
    /// Resolves final component bounds from arranged descendant margin bounds.
    /// </summary>
    /// <param name="innerContentBounds">
    /// Arranged descendant margin bounds, or <see langword="null"/> for empty content.
    /// </param>
    public GuiBounds ResolveBounds(
        GuiBounds availableBounds,
        GuiBounds? innerContentBounds)
        => ResolveBounds(
            availableBounds,
            ResolveContentExtent(innerContentBounds) + Padding,
            useAvailableSize: false);

    private static GuiSize ResolveContentExtent(
        GuiBounds? contentBounds)
    {
        var contentSize =
            contentBounds?.Size
            ?? new GuiSize(0, 0);

        if (contentBounds?.Position is not GuiPoint position
            || position.IsAbsolute)
        {
            return contentSize;
        }

        return new GuiSize(
            position.X + contentSize.Width,
            position.Y + contentSize.Height);
    }

    private GuiBounds ResolveBounds(
        GuiBounds availableBounds,
        GuiSize? candidateSize,
        bool useAvailableSize)
    {
        GuiBounds adjustedAvailableBounds =
            availableBounds.Consume(Margin);

        GuiSize? availableSize =
            adjustedAvailableBounds.Size;

        if (useAvailableSize)
        {
            candidateSize = availableSize;
        }

        var resolvedSize = new GuiSize(
            ResolveLength(
                availableSize?.Width,
                candidateSize?.Width,
                Width,
                MinimumWidth,
                MaximumWidth),
            ResolveLength(
                availableSize?.Height,
                candidateSize?.Height,
                Height,
                MinimumHeight,
                MaximumHeight));

        return new GuiBounds(
            ResolvePosition(
                adjustedAvailableBounds,
                resolvedSize),
            resolvedSize,
            Margin,
            Padding);
    }

    private GuiPoint? ResolvePosition(
        GuiBounds availableBounds,
        GuiSize resolvedSize)
    {
        var alignedPosition =
            ResolveAlignedPosition(
                availableBounds,
                resolvedSize);

        if (Position is null)
        {
            return alignedPosition;
        }

        if (Position.Value.IsAbsolute)
        {
            return Position;
        }

        if (alignedPosition is null)
        {
            return null;
        }

        return alignedPosition.Value + Position.Value;
    }

    private GuiPoint? ResolveAlignedPosition(
        GuiBounds availableBounds,
        GuiSize resolvedSize)
    {
        if (availableBounds.Position is null)
        {
            return null;
        }

        var horizontalOffset = ResolveAlignmentOffset(
            availableBounds.Size?.Width,
            resolvedSize.Width,
            HorizontalAlignment);

        var verticalOffset = ResolveAlignmentOffset(
            availableBounds.Size?.Height,
            resolvedSize.Height,
            VerticalAlignment);

        if (horizontalOffset is null || verticalOffset is null)
        {
            return null;
        }

        return availableBounds.Position.Value
            + new GuiPoint(
                horizontalOffset.Value,
                verticalOffset.Value);
    }

    private static double? ResolveAlignmentOffset(
        double? availableLength,
        double? resolvedLength,
        GuiAlignment alignment)
    {
        if (alignment is not GuiAlignment.Start
            and not GuiAlignment.Center
            and not GuiAlignment.End)
        {
            throw new ArgumentOutOfRangeException(
                nameof(alignment),
                alignment,
                "Unknown GUI alignment.");
        }

        if (alignment == GuiAlignment.Start)
        {
            return 0;
        }

        if (availableLength is null || resolvedLength is null)
        {
            return null;
        }

        var remainingLength =
            availableLength.Value - resolvedLength.Value;

        return alignment == GuiAlignment.Center
            ? remainingLength / 2
            : remainingLength;
    }

    private static double? ResolveLength(
        double? availableLength,
        double? candidateLength,
        GuiLengthRule? explicitRule,
        GuiLengthRule? minimumRule,
        GuiLengthRule? maximumRule)
        => GuiLengthRule.Resolve(
            availableLength,
            candidateLength,
            explicitRule,
            minimumRule,
            maximumRule);

    /// <summary>
    /// Resets all properties to their documented defaults. Called by the reconciler on
    /// reused component slots before applying the current pass's configuration actions so
    /// that each blueprint pass declares a full, fresh state rather than a delta on top of
    /// the previous pass.
    /// </summary>
    internal void Reset()
    {
        Position = null;
        Margin = GuiThickness.Zero;
        Padding = GuiThickness.Zero;
        MinimumWidth = null;
        Width = null;
        MaximumWidth = null;
        MinimumHeight = null;
        Height = null;
        MaximumHeight = null;
        HorizontalAlignment = GuiAlignment.Start;
        VerticalAlignment = GuiAlignment.Start;
    }
}
