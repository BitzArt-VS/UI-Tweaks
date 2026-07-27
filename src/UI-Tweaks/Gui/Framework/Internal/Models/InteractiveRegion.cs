namespace BitzArt.UI.Tweaks.Gui;

internal readonly struct InteractiveRegion
{
    public readonly GuiBounds Bounds;
    public readonly object Token;

    public readonly GuiCallback<GuiMouseEventArgs> OnMouseDown;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseUp;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseClick;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseMove;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseEnter;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseLeave;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseWheel;

    public InteractiveRegion(
        GuiBounds bounds,
        object token,
        GuiCallback<GuiMouseEventArgs> onMouseDown,
        GuiCallback<GuiMouseEventArgs> onMouseUp,
        GuiCallback<GuiMouseEventArgs> onMouseClick,
        GuiCallback<GuiMouseEventArgs> onMouseMove,
        GuiCallback<GuiMouseEventArgs> onMouseEnter,
        GuiCallback<GuiMouseEventArgs> onMouseLeave,
        GuiCallback<GuiMouseEventArgs> onMouseWheel = default)
    {
        Bounds = bounds;
        Token = token;
        OnMouseDown = onMouseDown;
        OnMouseUp = onMouseUp;
        OnMouseClick = onMouseClick;
        OnMouseMove = onMouseMove;
        OnMouseEnter = onMouseEnter;
        OnMouseLeave = onMouseLeave;
        OnMouseWheel = onMouseWheel;
    }

    public bool HasClickHandlers =>
        OnMouseDown.HasHandler || OnMouseUp.HasHandler || OnMouseClick.HasHandler
        || OnMouseMove.HasHandler || OnMouseEnter.HasHandler || OnMouseLeave.HasHandler;

    public bool Contains(double x, double y)
    {
        var position = Bounds.Position!.Value;
        var size = Bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        return x >= position.X && x < position.X + width
            && y >= position.Y && y < position.Y + height;
    }

    public InteractiveRegion Translated(double dx, double dy)
    {
        var position = Bounds.Position!.Value;
        var size = Bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        var translatedBounds = new GuiBounds(
            new GuiPoint(position.X + dx, position.Y + dy),
            new GuiSize(width, height));

        return new(
            translatedBounds,
            Token,
            OnMouseDown,
            OnMouseUp,
            OnMouseClick,
            OnMouseMove,
            OnMouseEnter,
            OnMouseLeave,
            OnMouseWheel);
    }
}
