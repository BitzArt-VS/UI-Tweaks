namespace BitzArt.UI.Tweaks.Gui;

internal readonly struct KeyboardRegion
{
    public readonly object Token;
    public readonly GuiCallback<GuiKeyEventArgs> OnKeyDown;
    public readonly GuiCallback<GuiKeyEventArgs> OnKeyUp;
    public readonly GuiCallback<GuiKeyEventArgs> OnKeyPress;
    public readonly GuiCallback<bool> OnFocusChanged;

    public KeyboardRegion(
        object token,
        GuiCallback<GuiKeyEventArgs> onKeyDown,
        GuiCallback<GuiKeyEventArgs> onKeyUp,
        GuiCallback<GuiKeyEventArgs> onKeyPress,
        GuiCallback<bool> onFocusChanged)
    {
        Token = token;
        OnKeyDown = onKeyDown;
        OnKeyUp = onKeyUp;
        OnKeyPress = onKeyPress;
        OnFocusChanged = onFocusChanged;
    }

    public void Dispatch(GuiKeyEventKind kind, GuiKeyEventArgs args)
    {
        switch (kind)
        {
            case GuiKeyEventKind.Down: OnKeyDown.Invoke(args); break;
            case GuiKeyEventKind.Up: OnKeyUp.Invoke(args); break;
            case GuiKeyEventKind.Press: OnKeyPress.Invoke(args); break;
        }
    }
}
