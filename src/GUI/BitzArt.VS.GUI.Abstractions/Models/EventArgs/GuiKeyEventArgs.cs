using Vintagestory.API.Client;

namespace BitzArt.VS.GUI;

public readonly struct GuiKeyEventArgs
{
    public KeyEvent Event { get; }

    public GuiKeyEventArgs(KeyEvent ev) => Event = ev;

    public int KeyCode => Event.KeyCode;

    public char KeyChar => Event.KeyChar;

    public bool ShiftPressed => Event.ShiftPressed;
    public bool CtrlPressed => Event.CtrlPressed;
    public bool AltPressed => Event.AltPressed;
    public bool CommandPressed => Event.CommandPressed;

    public bool Handled
    {
        get => Event.Handled;
        set => Event.Handled = value;
    }
}
