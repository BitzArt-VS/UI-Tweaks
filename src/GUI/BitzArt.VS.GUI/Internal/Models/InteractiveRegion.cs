namespace BitzArt.VS.GUI;

internal readonly struct InteractiveRegion
{
    public readonly GuiBounds Bounds;
    public readonly GuiBounds? ClipBounds;
    public readonly object Token;

    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseDown;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseUp;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseClick;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseMove;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseEnter;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseLeave;
    public readonly GuiCallback<GuiMouseEventArgs>? OnMouseWheel;
    public readonly IGuiNode? MouseDownFocusTarget;
    private readonly Predicate<GuiMouseEventArgs>? _mouseDownFocusCondition;

    public InteractiveRegion(
        GuiBounds bounds,
        object token,
        GuiCallback<GuiMouseEventArgs>? onMouseDown,
        GuiCallback<GuiMouseEventArgs>? onMouseUp,
        GuiCallback<GuiMouseEventArgs>? onMouseClick,
        GuiCallback<GuiMouseEventArgs>? onMouseMove,
        GuiCallback<GuiMouseEventArgs>? onMouseEnter,
        GuiCallback<GuiMouseEventArgs>? onMouseLeave,
        GuiCallback<GuiMouseEventArgs>? onMouseWheel = null,
        IGuiNode? mouseDownFocusTarget = null,
        Predicate<GuiMouseEventArgs>? mouseDownFocusCondition = null,
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
        MouseDownFocusTarget = mouseDownFocusTarget;
        _mouseDownFocusCondition = mouseDownFocusCondition;
    }

    public bool HasClickHandlers =>
        OnMouseDown is not null || OnMouseUp is not null || OnMouseClick is not null
        || OnMouseMove is not null || OnMouseEnter is not null || OnMouseLeave is not null;

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

    public bool ShouldRequestFocus(GuiMouseEventArgs args) =>
        MouseDownFocusTarget is not null
        && (_mouseDownFocusCondition is null || _mouseDownFocusCondition.Invoke(args));

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
            MouseDownFocusTarget,
            _mouseDownFocusCondition,
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
