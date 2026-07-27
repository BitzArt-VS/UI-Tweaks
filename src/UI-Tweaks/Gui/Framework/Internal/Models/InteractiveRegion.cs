namespace BitzArt.UI.Tweaks.Gui;

internal readonly struct InteractiveRegion
{
    public readonly GuiBounds Bounds;
    public readonly GuiBounds? ClipBounds;
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
        GuiCallback<GuiMouseEventArgs> onMouseWheel = default,
        GuiBounds? clipBounds = null)
    {
        Bounds = bounds;
        ClipBounds = clipBounds;
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
        var point =
            new GuiPoint(
                x,
                y,
                IsAbsolute: true);

        return Bounds.Contains(point)
            && (ClipBounds is not GuiBounds clipBounds
                || clipBounds.Contains(point));
    }

    public InteractiveRegion Translated(double dx, double dy)
    {
        return new(
            Translate(Bounds, dx, dy),
            Token,
            OnMouseDown,
            OnMouseUp,
            OnMouseClick,
            OnMouseMove,
            OnMouseEnter,
            OnMouseLeave,
            OnMouseWheel,
            ClipBounds is GuiBounds clipBounds
                ? Translate(clipBounds, dx, dy)
                : null);
    }

    private static GuiBounds Translate(GuiBounds bounds, double dx, double dy)
    {
        var position = bounds.Position!.Value;

        return bounds with
        {
            Position = new GuiPoint(
                position.X + dx,
                position.Y + dy,
                IsAbsolute: true),
        };
    }
}
