namespace BitzArt.VS.GUI;

/// <summary>
/// Slot-level mouse-event registration extensions. Attach a handler to any component slot
/// during <c>BuildComponentTree</c>. Handlers receive a <see cref="GuiMouseEventArgs"/> in
/// dialog-local logical coordinates and are wired to the slot's allocated bounds — there is
/// no need to scan child trees or forward events manually.
/// <para>
/// Every registration accepts a <see cref="GuiCallback{T}"/>, the framework's common
/// synchronous-or-asynchronous callback contract.
/// </para>
/// <para>
/// Semantics:
/// <list type="bullet">
///   <item><c>OnMouseDown</c> fires when the cursor presses inside the slot.</item>
///   <item><c>OnMouseUp</c> fires on the slot that received <c>OnMouseDown</c>, regardless of
///   where the release lands — mouse capture is implicit, mirroring typical UI behaviour
///   (Win32 SetCapture, browsers, vanilla VS buttons).</item>
///   <item><c>OnMouseClick</c> fires only when both press and release land inside the slot's
///   bounds.</item>
///   <item><c>OnMouseMove</c> fires while the mouse is captured by this slot (i.e. between
///   <c>OnMouseDown</c> and <c>OnMouseUp</c>) — including while the cursor is outside the
///   slot's bounds. It is the basis for drag interactions.</item>
///   <item><c>OnMouseEnter</c> fires once when the uncaptured cursor first moves over the
///   slot's bounds. Basis for hover overlays and tooltips.</item>
///   <item><c>OnMouseLeave</c> fires once when the cursor leaves the slot's bounds (or the
///   dialog entirely) after a previous <c>OnMouseEnter</c>. Always paired with Enter.</item>
///   <item><c>OnMouseWheel</c> fires on the last matching wheel-enabled slot under the
///   pointer.</item>
/// </list>
/// </para>
/// </summary>
public static class MouseEventBuilderExtensions
{
    public static TBuilder OnMouseDown<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Down, callback);
        return builder;
    }

    public static TBuilder OnMouseUp<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Up, callback);
        return builder;
    }

    public static TBuilder OnMouseClick<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Click, callback);
        return builder;
    }

    public static TBuilder OnMouseMove<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Move, callback);
        return builder;
    }

    public static TBuilder OnMouseEnter<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Enter, callback);
        return builder;
    }

    public static TBuilder OnMouseLeave<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Leave, callback);
        return builder;
    }

    public static TBuilder OnMouseWheel<TBuilder>(this TBuilder builder, GuiCallback<GuiMouseEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddMouseHandler(GuiMouseEventKind.Wheel, callback);
        return builder;
    }
}

