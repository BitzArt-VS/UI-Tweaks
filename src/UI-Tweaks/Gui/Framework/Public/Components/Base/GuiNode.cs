using Cairo;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Default base-class implementation of <see cref="IGuiNode"/>. Wires up the mounted slot,
/// render fragment, lifecycle hooks, cascading-value resolution, and an empty
/// <see cref="BuildRenderTree"/> hook for declaring children.
/// <para>
/// Layout-participating components inherit from <see cref="GuiComponent"/> (which extends
/// this base with <see cref="GuiComponent.LayoutParameters"/> and
/// <see cref="GuiComponent.Measure"/>). Pure layout-transparent wrappers — tooltips,
/// focus trackers, debug overlays — inherit from this base directly.
/// </para>
/// </summary>
public abstract class GuiNode : IGuiNode
{
    public GuiRenderFragment RenderFragment { get; }

    protected IGuiNodeSlot? Slot { get; private set; }
    protected ICoreClientAPI? ClientApi => Slot?.ClientApi;

    protected GuiNode()
    {
        RenderFragment = builder => BuildRenderTree(builder);
    }

    public void Attach(IGuiNodeSlot slot)
    {
        Slot = slot;
    }

    /// <summary>
    /// Requests reconciliation of this node's render fragment. Reconciliation cascades into
    /// layout and rendering.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the node is not attached to a slot.</exception>
    protected void RequestReconcile()
    {
        GetAttachedSlot(nameof(RequestReconcile)).RequestReconcile();
    }

    /// <summary>
    /// Returns the cascading value of type <typeparamref name="T"/> published by the
    /// nearest ancestor <see cref="CascadingValue{T}"/> with no <c>Name</c>, or
    /// <c>default</c> when no matching provider exists.
    /// </summary>
    /// <remarks>
    /// Mirrors Blazor's <c>[CascadingParameter]</c> resolution. Typically called from
    /// <see cref="OnParametersSet"/> to snapshot the value into a field; it is also safe
    /// to call from <see cref="Render"/> / <see cref="RenderOverlay"/> for nodes that
    /// prefer to read the latest value directly each frame.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the node is not yet attached to a slot (i.e. before
    /// <see cref="Attach"/> has run).
    /// </exception>
    protected T? GetCascadingValue<T>()
        => GetCascadingValue<T>(name: null);

    /// <inheritdoc cref="GetCascadingValue{T}()"/>
    /// <param name="name">
    /// Discriminator matching <see cref="CascadingValue{T}.Name"/>. <c>null</c> matches
    /// only providers with no name; non-null names compare ordinally.
    /// </param>
    protected T? GetCascadingValue<T>(string? name)
    {
        if (Slot is null)
        {
            throw new InvalidOperationException("Cannot resolve a cascading value before the node is attached to a slot.");
        }

        return Slot.TryGetCascadingValue<T>(name, out var value) ? value : default;
    }

    /// <summary>
    /// Attempts to resolve an unnamed cascading value of type <typeparamref name="T"/>.
    /// Returns <c>false</c> when no matching provider exists, leaving <paramref name="value"/>
    /// at <c>default</c> — use this overload when "no provider" must be distinguished
    /// from "provider published <c>default(T)</c>".
    /// </summary>
    protected bool TryGetCascadingValue<T>(out T value)
        => TryGetCascadingValue(name: null, out value);

    /// <inheritdoc cref="TryGetCascadingValue{T}(out T)"/>
    /// <param name="name">Discriminator matching <see cref="CascadingValue{T}.Name"/>.</param>
    protected bool TryGetCascadingValue<T>(string? name, out T value)
    {
        if (Slot is null)
        {
            throw new InvalidOperationException("Cannot resolve a cascading value before the node is attached to a slot.");
        }

        return Slot.TryGetCascadingValue(name, out value);
    }

    /// <inheritdoc/>
    public virtual void OnInitialized() { }

    /// <inheritdoc/>
    public virtual void OnParametersSet() { }

    internal void ApplySlotConfiguration(IGuiSlotBuilder builder) => ConfigureSlot(builder);

    protected virtual void ConfigureSlot(IGuiSlotBuilder builder) { }

    /// <inheritdoc/>
    public virtual void OnFrame(float deltaTime) { }

    /// <summary>
    /// Override this method to build the render tree for this node using the provided builder.
    /// </summary>
    /// <param name="builder">Builder for constructing internal render tree of this node.</param>
    protected virtual void BuildRenderTree(IGuiRenderTreeBuilder builder) { }

    /// <summary>
    /// Override this method to perform custom rendering using the provided Cairo context and bounds.
    /// </summary>
    /// <param name="context">Cairo context for drawing operations.</param>
    /// <param name="bounds">Node bounds defining the area available for rendering.</param>
    public virtual void Render(Context context, GuiComponentBounds bounds) { }

    /// <summary>
    /// Override this method to render overlays on top of this node's children.
    /// Called after this node's own children have been rendered, but before any
    /// later sibling slot begins drawing.
    /// </summary>
    /// <param name="context">Cairo context for drawing operations.</param>
    /// <param name="bounds">Node bounds defining the area available for rendering.</param>
    public virtual void RenderOverlay(Context context, GuiComponentBounds bounds) { }

    protected IGuiNodeSlot GetAttachedSlot(string memberName)
    {
        if (Slot is null)
        {
            throw new InvalidOperationException($"Cannot access {memberName} on a node that is not attached to a slot.");
        }

        return Slot;
    }
}
