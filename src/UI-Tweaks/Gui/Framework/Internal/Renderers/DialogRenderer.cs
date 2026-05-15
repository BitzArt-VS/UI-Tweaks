using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks.Gui;

internal sealed class DialogRenderer : GuiSurfaceRenderer, IFloatingLayerInputHost
{
    private readonly IGuiDialog _dialog;
    private readonly GuiCallback<GuiMouseEventArgs> _onDialogMouseDown;
    private readonly GuiCallback<GuiMouseEventArgs> _onDialogMouseUp;
    private readonly GuiCallback<GuiMouseEventArgs> _onDialogMouseMove;
    private readonly GuiCallback<GuiMouseEventArgs> _onDialogMouseLeave;
    private bool _isDisposed;
    private bool _isFocused;

    private readonly CairoDialogInputInterceptor _inputInterceptor;

    private double _currentLogicalWidth;
    private double _currentLogicalHeight;

    private readonly ScopedRebuildQueue _rebuildQueue = new();
    private readonly DialogInputDispatcher _inputDispatcher;

    private readonly DialogScreenProjection _screenProjection;

    private readonly FloatingLayerRenderer _tooltipLayer;
    private readonly TooltipHost _tooltipHost;
    private readonly FloatingLayerRenderer _overlayLayer;
    private readonly OverlayHost _overlayHost;
    private readonly GuiCursorHost _cursorHost = new();
    private string? _dialogOverrideCursor;

    private readonly FocusManager _focusManager;
    private readonly IFloatingLayer[] _floatingLayers;

    internal GuiCursorHost CursorHost => _cursorHost;
    internal IGuiNode? FocusedNode => _inputDispatcher.FocusedNode;

    public override double RenderOrder => _dialog.RenderOrder;

    internal DialogRenderer(ICoreClientAPI clientApi, IGuiDialog dialog, string name)
        : base(clientApi)
    {
        _dialog = dialog;
        _onDialogMouseDown = new Action<GuiMouseEventArgs>(dialog.OnMouseDown);
        _onDialogMouseUp = new Action<GuiMouseEventArgs>(dialog.OnMouseUp);
        _onDialogMouseMove = new Action<GuiMouseEventArgs>(dialog.OnMouseMove);
        _onDialogMouseLeave = new Action<GuiMouseEventArgs>(dialog.OnMouseLeave);

        if (dialog.LayoutParameters.Width.IsAuto || dialog.LayoutParameters.Height.IsAuto)
            throw new ArgumentException("Dialog must have fixed width and height for rendering.", nameof(dialog));

        _currentLogicalWidth = dialog.LayoutParameters.Width.Value;
        _currentLogicalHeight = dialog.LayoutParameters.Height.Value;

        EnsureSurfaceSize(
            (int)Math.Round(_currentLogicalWidth * _currentScale),
            (int)Math.Round(_currentLogicalHeight * _currentScale));

        _tooltipLayer = new FloatingLayerRenderer(clientApi);
        _overlayLayer = new FloatingLayerRenderer(clientApi);
        _screenProjection = new DialogScreenProjection(clientApi, dialog);
        _tooltipHost = new TooltipHost(_tooltipLayer);
        _overlayHost = new OverlayHost(_overlayLayer, this, _screenProjection);
        _floatingLayers = [_overlayLayer, _tooltipLayer];
        _focusManager = new FocusManager(this);

        _inputDispatcher = new DialogInputDispatcher(_screenProjection.TryToLogical, _tooltipHost);

        Builder.CascadeChain = BuildRootCascadeChain();
        _tooltipLayer.SetCascadeChain(Builder.CascadeChain);
        _overlayLayer.SetCascadeChain(Builder.CascadeChain);

        _inputInterceptor = new CairoDialogInputInterceptor(clientApi, this);
        clientApi.Gui.RegisterDialog(_inputInterceptor);

        RequestArrange();
    }

    private CascadingValueChain BuildRootCascadeChain()
    {
        var chain = new CascadingValueChain(parent: null, valueType: typeof(TooltipHost), name: null, value: _tooltipHost);
        chain = new CascadingValueChain(parent: chain, valueType: typeof(FocusManager), name: null, value: _focusManager);
        chain = new CascadingValueChain(parent: chain, valueType: typeof(OverlayHost), name: null, value: _overlayHost);
        chain = new CascadingValueChain(parent: chain, valueType: typeof(GuiCursorHost), name: null, value: _cursorHost);
        return chain;
    }

    public override void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        if (_isDisposed) return;

        _inputDispatcher.FocusedNode?.OnFrame(deltaTime);
        if (_rebuildQueue.Drain())
        {
            RequestArrange();
        }
        RequestSurfaceUpdateForScaleOrSizeChanges();
        if (_arrangeRequested)
        {
            ExecuteArrangeWalk();
        }
        else if (_paintRequested)
        {
            ExecutePaintWalk();
        }

        var (posX, posY) = _screenProjection.GetScreenOrigin();
        BlitAt(posX, posY);

        for (int i = 0; i < _floatingLayers.Length; i++)
            _floatingLayers[i].Render();
    }

    private void RequestSurfaceUpdateForScaleOrSizeChanges()
    {
        float scale = RuntimeEnv.GUIScale;
        double logicalWidth = _dialog.LayoutParameters.Width.Value;
        double logicalHeight = _dialog.LayoutParameters.Height.Value;

        if (scale == _currentScale && logicalWidth == _currentLogicalWidth && logicalHeight == _currentLogicalHeight)
        {
            return;
        }

        bool logicalSizeChanged = logicalWidth != _currentLogicalWidth || logicalHeight != _currentLogicalHeight;
        _currentScale = scale;
        _currentLogicalWidth = logicalWidth;
        _currentLogicalHeight = logicalHeight;
        EnsureSurfaceSize(
            (int)Math.Round(logicalWidth * scale),
            (int)Math.Round(logicalHeight * scale));

        RequestPaint();
    }

    private void ExecuteArrangeWalk()
    {
        _inputDispatcher.ClearArrangedRegions();
        _tooltipHost.ResetFrame();

        for (int i = 0; i < _floatingLayers.Length; i++)
            _floatingLayers[i].OnFrameStart();

        // Register the dialog as the lowest-z-order background region so any click inside
        // the dialog bounds that misses all component regions hits the dialog itself. Added
        // first (index 0) so the reverse-order hit-test always prefers components over it.
        _inputDispatcher.AddInteractiveRegion(new InteractiveRegion(
            new GuiComponentBounds(0, 0, _currentLogicalWidth, _currentLogicalHeight),
            _dialog,
            onMouseDown: _onDialogMouseDown,
            onMouseUp: _onDialogMouseUp,
            onMouseClick: default,
            onMouseMove: _onDialogMouseMove,
            onMouseEnter: default,
            onMouseLeave: _onDialogMouseLeave));

        var bounds = new GuiComponentBounds(0, 0, _currentLogicalWidth, _currentLogicalHeight);
        DrawSurfaceContents(bounds, _dialog.LayoutParameters.Direction, _currentScale, arrange: true);

        for (int i = 0; i < _floatingLayers.Length; i++)
            _floatingLayers[i].RunWalk();

        _inputDispatcher.RefreshHoverIfNotCapturing(_clientApi.Input.MouseX, _clientApi.Input.MouseY);
    }

    private void ExecutePaintWalk()
    {
        _tooltipHost.ResetFrame();

        var bounds = new GuiComponentBounds(0, 0, _currentLogicalWidth, _currentLogicalHeight);
        DrawSurfaceContents(bounds, _dialog.LayoutParameters.Direction, _currentScale, arrange: false);

        _inputDispatcher.RefreshHoverIfNotCapturing(_clientApi.Input.MouseX, _clientApi.Input.MouseY);
    }

    public override void Schedule(GuiRenderFragment fragment, GuiRenderTreeBuilder builder)
    {
        if (_isDisposed) return;
        _rebuildQueue.Schedule(fragment, builder);
        RequestReconcile();
    }

    public override void Cancel(GuiRenderFragment fragment) => _rebuildQueue.Cancel(fragment);

    public override void AddInteractiveRegion(in InteractiveRegion region) => _inputDispatcher.AddInteractiveRegion(region);
    public override void AddKeyboardRegion(in KeyboardRegion region) => _inputDispatcher.AddKeyboardRegion(region);

    // --- Lifecycle ---

    internal void TryOpen() => _inputInterceptor.TryOpen();
    internal void TryClose() => _inputInterceptor.TryClose();

    internal void RequestFocus() => _clientApi.Gui.RequestFocus(_inputInterceptor);

    internal void SetMouseOverCursor(string? cursor)
    {
        _dialogOverrideCursor = cursor;
        // Set immediately on the interceptor so the cursor is correct even when the
        // mouse is stationary (e.g. holding down at the start of a resize gesture).
        _inputInterceptor.MouseOverCursor = cursor;
    }

    // --- Focus forwarding ---

    internal void OnFocus()
    {
        _isFocused = true;
        _dialog.OnFocus();
    }

    internal void OnUnFocus()
    {
        _isFocused = false;
        _dialog.OnUnFocus();
    }

    internal bool OnEscapePressed() => _dialog.OnEscapePressed();

    // --- Full event handlers (called directly by the interceptor) ---

    internal void OnMouseDown(MouseEvent args)
    {
        if (args.Handled) return;
        bool hit = _inputDispatcher.DispatchMouseDown(args);
        if (hit)
        {
            RequestFocus();
            args.Handled = true;
        }
    }

    internal void OnMouseUp(MouseEvent args)
    {
        if (args.Handled) return;
        _inputDispatcher.DispatchMouseUp(args);
        if (ContainsScreenPoint(args.X, args.Y) || ContainsOverlayScreenPoint(args.X, args.Y))
        {
            args.Handled = true;
        }
    }

    internal void OnMouseMove(MouseEvent args)
    {
        if (args.Handled) return;
        bool dispatched = _inputDispatcher.DispatchMouseMove(args);
        // Resize cursor (set via SetMouseOverCursor during dispatch) takes priority over
        // any component hover cursor. Fall back to the hover cursor when not resizing.
        _inputInterceptor.MouseOverCursor = _dialogOverrideCursor ?? _cursorHost.HoverCursor;
        if (dispatched || ContainsScreenPoint(args.X, args.Y) || ContainsOverlayScreenPoint(args.X, args.Y))
        {
            args.Handled = true;
        }
    }

    internal void OnMouseWheel(MouseWheelEventArgs args)
    {
        if (args.IsHandled) return;
        if (_isFocused
            && (ContainsScreenPoint(_clientApi.Input.MouseX, _clientApi.Input.MouseY)
                || ContainsOverlayScreenPoint(_clientApi.Input.MouseX, _clientApi.Input.MouseY)))
        {
            _inputDispatcher.DispatchMouseWheel(_clientApi.Input.MouseX, _clientApi.Input.MouseY, args.deltaPrecise);
            args.SetHandled(true);
        }
    }

    internal void OnKeyDown(KeyEvent args)
    {
        if (args.Handled) return;
        _dialog.OnKeyDown(args);
        if (args.Handled) return;
        _inputDispatcher.DispatchKeyDown(args);
    }

    internal void OnKeyUp(KeyEvent args)
    {
        if (args.Handled) return;
        _dialog.OnKeyUp(args);
        if (args.Handled) return;
        _inputDispatcher.DispatchKeyUp(args);
    }

    internal void OnKeyPress(KeyEvent args)
    {
        if (args.Handled) return;
        _dialog.OnKeyPress(args);
        if (args.Handled) return;
        _inputDispatcher.DispatchKeyPress(args);
    }

    // --- Geometry helpers (used by GuiDialog for resize hit-testing and overlay checks) ---

    public override bool ContainsScreenPoint(int x, int y) => _screenProjection.Contains(x, y);
    public bool ContainsOverlayScreenPoint(int x, int y) => _overlayLayer.ContainsScreenPoint(x, y);

    internal (int posX, int posY) GetScreenOrigin() => _screenProjection.GetScreenOrigin();

    public bool TryToLogical(int x, int y, out double logicalX, out double logicalY) =>
        _screenProjection.TryToLogical(x, y, out logicalX, out logicalY);

    internal void SetFocusedNode(IGuiNode? node) => _inputDispatcher.SetFocusedNode(node);

    internal void HideTooltip() => _tooltipHost.Hide();

    public override void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _clientApi.UnregisterDialog(_inputInterceptor);
        _inputInterceptor.Dispose();
        for (int i = 0; i < _floatingLayers.Length; i++)
            _floatingLayers[i].Dispose();
        base.Dispose();
    }
}
