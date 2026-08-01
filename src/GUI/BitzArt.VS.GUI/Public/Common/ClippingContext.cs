namespace BitzArt.VS.GUI;

/// <summary>
/// Per-surface clipping service available to components as a cascading value.
/// </summary>
public sealed class ClippingContext
{
    private readonly Dictionary<IGuiNode, GuiBounds> _clips =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Sets resolved bounds that clip descendants of <paramref name="owner"/> during
    /// rendering and pointer hit testing. Nested clips intersect. Passing
    /// <see langword="null"/> removes the owner's clip.
    /// </summary>
    /// <remarks>
    /// Components with layout-dependent clips should call this during every
    /// <see cref="IGuiComponent.Arrange"/> pass.
    /// </remarks>
    public void SetClip(IGuiNode owner, GuiBounds? bounds)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (bounds is null)
        {
            _clips.Remove(owner);
            return;
        }

        var clipBounds = bounds.Value;
        if (clipBounds.Position?.IsAbsolute != true
            || clipBounds.Size?.Width is null
            || clipBounds.Size?.Height is null)
        {
            throw new ArgumentException(
                "Clip bounds must have an absolute position and resolved size.",
                nameof(bounds));
        }

        _clips[owner] = clipBounds;
    }

    internal void Reset() => _clips.Clear();

    internal bool TryGetClip(IGuiNode owner, out GuiBounds bounds)
        => _clips.TryGetValue(owner, out bounds);
}
