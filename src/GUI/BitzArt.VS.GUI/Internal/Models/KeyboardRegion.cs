namespace BitzArt.VS.GUI;

internal readonly struct KeyboardRegion
{
    public readonly object Token;
    public readonly GuiCallback<GuiKeyEventArgs>? OnKeyDown;
    public readonly GuiCallback<GuiKeyEventArgs>? OnKeyUp;
    public readonly GuiCallback<GuiKeyEventArgs>? OnKeyPress;
    public readonly GuiCallback<bool>? OnFocusChanged;

    public KeyboardRegion(
        object token,
        GuiCallback<GuiKeyEventArgs>? onKeyDown,
        GuiCallback<GuiKeyEventArgs>? onKeyUp,
        GuiCallback<GuiKeyEventArgs>? onKeyPress,
        GuiCallback<bool>? onFocusChanged)
    {
        Token = token;
        OnKeyDown = onKeyDown;
        OnKeyUp = onKeyUp;
        OnKeyPress = onKeyPress;
        OnFocusChanged = onFocusChanged;
    }

    public void Dispatch(
        GuiKeyEventKind kind,
        GuiKeyEventArgs args,
        GuiCallbackDispatcher callbackDispatcher)
    {
        switch (kind)
        {
            case GuiKeyEventKind.Down:
                callbackDispatcher.Dispatch(OnKeyDown, args);
                break;
            case GuiKeyEventKind.Up:
                callbackDispatcher.Dispatch(OnKeyUp, args);
                break;
            case GuiKeyEventKind.Press:
                callbackDispatcher.Dispatch(OnKeyPress, args);
                break;
        }
    }
}
