namespace BitzArt.VS.GUI;

/// <summary>
/// Slot-level keyboard-event registration extensions. Mirrors
/// <see cref="MouseEventBuilderExtensions"/> for the keyboard counterpart.
/// <para>
/// Unlike mouse events, keyboard events are not spatially routed. The root dialog slot
/// receives keyboard events first for dialog-wide shortcuts; unhandled events then fire
/// on the slot whose component currently holds focus (<see cref="FocusManager"/>). A
/// click on a focusable component requests focus; clicks elsewhere clear it.
/// </para>
/// <para>
/// Every registration accepts a <see cref="GuiCallback{T}"/>. Handlers may mark
/// <see cref="GuiKeyEventArgs.Handled"/> to suppress framework defaults (e.g. Escape
/// closing the dialog).
/// </para>
/// </summary>
public static class KeyEventBuilderExtensions
{
    public static TBuilder OnKeyDown<TBuilder>(this TBuilder builder, GuiCallback<GuiKeyEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddKeyHandler(GuiKeyEventKind.Down, callback);
        return builder;
    }

    public static TBuilder OnKeyUp<TBuilder>(this TBuilder builder, GuiCallback<GuiKeyEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddKeyHandler(GuiKeyEventKind.Up, callback);
        return builder;
    }

    public static TBuilder OnKeyPress<TBuilder>(this TBuilder builder, GuiCallback<GuiKeyEventArgs> callback)
        where TBuilder : IGuiSlotBuilder
    {
        builder.AddKeyHandler(GuiKeyEventKind.Press, callback);
        return builder;
    }
}
