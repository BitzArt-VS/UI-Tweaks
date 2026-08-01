namespace BitzArt.VS.GUI;

/// <summary>
/// Slot-level focus-event registration extensions. Focus changes are reference-routed:
/// handlers fire when the slot's node becomes or stops being the focused node. On the
/// root dialog slot, handlers track native dialog/surface focus instead.
/// </summary>
public static class FocusEventBuilderExtensions
{
    /// <summary>
    /// Declares the node that receives focus synchronously when this slot receives a
    /// matching mouse-down event.
    /// </summary>
    /// <param name="builder">Slot declaration receiving the focus behavior.</param>
    /// <param name="target">Node to focus.</param>
    /// <param name="condition">
    /// Optional condition evaluated during mouse routing. When it returns
    /// <see langword="false"/>, focus is cleared instead.
    /// </param>
    /// <returns>The same builder for fluent configuration.</returns>
    public static TBuilder FocusOnMouseDown<TBuilder>(
        this TBuilder builder,
        IGuiNode target,
        Predicate<GuiMouseEventArgs>? condition = null)
        where TBuilder : IGuiSlotBuilder
    {
        builder.SetMouseDownFocusTarget(target, condition);
        return builder;
    }

    public static TBuilder OnFocusChanged<TBuilder>(this TBuilder builder, GuiCallback<bool> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddFocusChangedHandler(callback);
        return builder;
    }
}
