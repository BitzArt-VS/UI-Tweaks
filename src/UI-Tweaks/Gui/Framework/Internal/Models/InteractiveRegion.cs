namespace BitzArt.UI.Tweaks.Gui;

internal readonly struct InteractiveRegion
{
    public readonly GuiComponentBounds Bounds;
    public readonly object Token;

    public readonly GuiCallback<GuiMouseEventArgs> OnMouseDown;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseUp;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseClick;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseMove;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseEnter;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseLeave;
    public readonly GuiCallback<GuiMouseEventArgs> OnMouseWheel;

    public InteractiveRegion(
        GuiComponentBounds bounds,
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

    public bool Contains(double x, double y) =>
        x >= Bounds.X && x < Bounds.X + Bounds.Width &&
        y >= Bounds.Y && y < Bounds.Y + Bounds.Height;

    public InteractiveRegion Translated(double dx, double dy) => new(
        new GuiComponentBounds(Bounds.X + dx, Bounds.Y + dy, Bounds.Width, Bounds.Height),
        Token, OnMouseDown, OnMouseUp, OnMouseClick, OnMouseMove, OnMouseEnter, OnMouseLeave,
        OnMouseWheel);
}
