namespace BitzArt.VS.GUI;

internal sealed class GuiResizeController
{
    private const double EdgeThickness = 6.0;

    private readonly Func<IGuiNode?> _getRootNode;
    private readonly Action<string?> _setMouseOverCursor;

    private bool _isResizing;
    private bool _useScreenBounds;
    private ResizeRegion _activeRegion;
    private GuiEdge _activeEdge;
    private GuiBounds _startBounds;
    private GuiPoint _startPointer;

    internal bool IsResizing => _isResizing;

    internal GuiResizeController(Func<IGuiNode?> getRootNode, Action<string?> setMouseOverCursor)
    {
        _getRootNode = getRootNode;
        _setMouseOverCursor = setMouseOverCursor;
    }

    internal bool TryBegin(IReadOnlyList<ResizeRegion> regions, GuiMouseEventArgs args)
    {
        if (_isResizing)
        {
            return false;
        }

        if (!TryHit(regions, args.Position.X, args.Position.Y, out var region, out var edge))
        {
            return false;
        }

        _isResizing = true;
        _activeRegion = region;
        _activeEdge = edge;
        _useScreenBounds = ReferenceEquals(region.Token, _getRootNode());
        _startBounds = _useScreenBounds ? ToScreenBounds(region.Bounds, args) : region.Bounds;
        _startPointer = _useScreenBounds ? args.AbsolutePosition : args.Position;

        _setMouseOverCursor.Invoke(CursorForEdge(edge));
        return true;
    }

    internal void Update(GuiMouseEventArgs args)
    {
        if (!_isResizing)
        {
            return;
        }

        var pointer = _useScreenBounds ? args.AbsolutePosition : args.Position;
        var delta = new GuiPoint(
            pointer.X - _startPointer.X,
            pointer.Y - _startPointer.Y);

        _activeRegion.Target.Resize(CreateRequestedBounds(delta));
    }

    internal void End()
    {
        if (!_isResizing)
        {
            return;
        }

        _isResizing = false;
        _activeRegion = default;
        _activeEdge = GuiEdge.None;
        _useScreenBounds = false;
        _setMouseOverCursor.Invoke(null);
    }

    internal bool UpdateHover(IReadOnlyList<ResizeRegion> regions, GuiMouseEventArgs args)
    {
        if (_isResizing)
        {
            return true;
        }

        if (!TryHit(regions, args.Position.X, args.Position.Y, out _, out var edge))
        {
            _setMouseOverCursor.Invoke(null);
            return false;
        }

        _setMouseOverCursor.Invoke(CursorForEdge(edge));
        return true;
    }

    private static bool TryHit(
        IReadOnlyList<ResizeRegion> regions,
        double x,
        double y,
        out ResizeRegion region,
        out GuiEdge edge)
    {
        for (int i = regions.Count - 1; i >= 0; i--)
        {
            region = regions[i];
            edge = HitTest(region, x, y);
            if (edge != GuiEdge.None)
            {
                return true;
            }
        }

        region = default;
        edge = GuiEdge.None;
        return false;
    }

    private static GuiEdge HitTest(ResizeRegion region, double x, double y)
    {
        if (!region.Contains(x, y))
        {
            return GuiEdge.None;
        }

        var supported = region.Target.SupportedResizeEdges;
        if (supported == GuiEdge.None)
        {
            return GuiEdge.None;
        }

        var position = region.Bounds.Position!.Value;
        var size = region.Bounds.Size!.Value;
        double localX = x - position.X;
        double localY = y - position.Y;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        var edge = GuiEdge.None;

        if ((supported & GuiEdge.Left) != 0 && localX < EdgeThickness)
        {
            edge |= GuiEdge.Left;
        }
        else if ((supported & GuiEdge.Right) != 0 && localX > width - EdgeThickness)
        {
            edge |= GuiEdge.Right;
        }

        if ((supported & GuiEdge.Top) != 0 && localY < EdgeThickness)
        {
            edge |= GuiEdge.Top;
        }
        else if ((supported & GuiEdge.Bottom) != 0 && localY > height - EdgeThickness)
        {
            edge |= GuiEdge.Bottom;
        }

        return edge;
    }

    private GuiBounds CreateRequestedBounds(GuiPoint delta)
    {
        var position = _startBounds.Position!.Value;
        var size = _startBounds.Size!.Value;
        double left = position.X;
        double top = position.Y;
        double right = left + size.Width!.Value;
        double bottom = top + size.Height!.Value;

        if ((_activeEdge & GuiEdge.Left) != 0)
        {
            left += delta.X;
        }
        else if ((_activeEdge & GuiEdge.Right) != 0)
        {
            right += delta.X;
        }

        if ((_activeEdge & GuiEdge.Top) != 0)
        {
            top += delta.Y;
        }
        else if ((_activeEdge & GuiEdge.Bottom) != 0)
        {
            bottom += delta.Y;
        }

        var requestedPosition = new GuiPoint(
            Math.Min(left, right),
            Math.Min(top, bottom),
            IsAbsolute: true);
        var requestedSize = new GuiSize(Math.Abs(right - left), Math.Abs(bottom - top));
        return new GuiBounds(requestedPosition, requestedSize);
    }

    private static GuiBounds ToScreenBounds(GuiBounds surfaceBounds, GuiMouseEventArgs args)
    {
        double surfaceScreenX = args.AbsolutePosition.X - args.Position.X;
        double surfaceScreenY = args.AbsolutePosition.Y - args.Position.Y;

        var surfacePosition = surfaceBounds.Position!.Value;
        var surfaceSize = surfaceBounds.Size!.Value;
        var screenPosition = new GuiPoint(
            surfaceScreenX + surfacePosition.X,
            surfaceScreenY + surfacePosition.Y,
            IsAbsolute: true);
        return new GuiBounds(screenPosition, surfaceSize);
    }

    private static string? CursorForEdge(GuiEdge edge)
    {
        if (edge is GuiEdge.Left or GuiEdge.Right)
        {
            return GuiResizeCursors.Horizontal;
        }

        if (edge is GuiEdge.Top or GuiEdge.Bottom)
        {
            return GuiResizeCursors.Vertical;
        }

        if (edge == (GuiEdge.Left | GuiEdge.Top)
            || edge == (GuiEdge.Right | GuiEdge.Bottom))
        {
            return GuiResizeCursors.DiagonalNwSe;
        }

        if (edge == (GuiEdge.Right | GuiEdge.Top)
            || edge == (GuiEdge.Left | GuiEdge.Bottom))
        {
            return GuiResizeCursors.DiagonalNeSw;
        }

        return null;
    }
}
