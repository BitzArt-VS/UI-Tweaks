using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.VS.GUI;

internal class FloatingLayerRenderer : GuiSurfaceRenderer
{
    protected GuiSize _arrangedSize;

    private object? _activeToken;
    private FloatingLayerPlacement _activePlacement;
    private bool _refreshedThisFrame;

    protected GuiTreeFragment? ActiveFragment { get; set; }

    internal bool IsActive => ActiveFragment is not null;

    private double ArrangedWidth =>
        _arrangedSize.Width ?? 0;

    private double ArrangedHeight =>
        _arrangedSize.Height ?? 0;

    public FloatingLayerRenderer(ICoreClientAPI clientApi) : base(clientApi) { }

    public void Show(object token, GuiTreeFragment content, in FloatingLayerPlacement placement)
    {
        _activeToken = token;
        ActiveFragment = content;
        _activePlacement = placement;
        _refreshedThisFrame = true;
        RequestReconcile();
    }

    public void Hide(object token)
    {
        if (!ReferenceEquals(_activeToken, token))
        {
            return;
        }

        ClearActive();
    }

    public void OnFrameStart() => _refreshedThisFrame = false;

    public void RunWalk()
    {
        if (ActiveFragment is not null
            && _activePlacement.AutoClearWhenNotRefreshed
            && !_refreshedThisFrame)
        {
            ClearActive();
        }

        if (!_activePlacement.RewalkOnDialogWalk)
        {
            return;
        }

        // The host's region tables were just cleared by the dialog's arrange walk —
        // even an unchanged layer must re-walk so its regions get re-registered.
        if (ActiveFragment is not null)
        {
            RequestReconcile();
        }
        Update();
    }

    public void Render()
    {
        // RunWalk handles dialog-driven arrangement, while this also drains invalidations
        // requested independently by nodes inside the floating layer.
        Update();
        Blit();
    }

    private void ClearActive()
    {
        if (ActiveFragment is null)
        {
            return;
        }

        _activeToken = null;
        ActiveFragment = null;
        _activePlacement = default;
    }

    protected void Update()
    {
        if (ActiveFragment is null)
        {
            return;
        }

        float scale = RuntimeEnv.GUIScale;
        if (!HasPendingSurfaceUpdate && scale == _currentScale)
        {
            return;
        }

        bool arrange = _arrangeRequested || scale != _currentScale;
        if (_reconcileRequested)
        {
            TreeBuilder.Run(BuildRootFragment);
        }

        if (arrange)
        {
            _arrangedSize = ResolveLogicalSize();
        }

        if (ArrangedWidth <= 0 || ArrangedHeight <= 0)
        {
            _currentScale = scale;
            ClearInvalidationRequests(arrange);
            return;
        }

        if (arrange)
        {
            ReallocateSurfaceIfNeeded(scale);
        }
        DrawToSurface(scale, arrange);
    }

    private void BuildRootFragment(IGuiTreeBuilder builder)
    {
        GuiLength? width = null;
        GuiLength? height = null;

        if (_activePlacement.FixedLogicalSize is GuiSize fixedSize)
        {
            width = fixedSize.Width is double fixedWidth
                ? fixedWidth
                : null;
            height = fixedSize.Height is double fixedHeight
                ? fixedHeight
                : null;
        }

        builder.Add<GuiContainer>(0)
            .Configure(container => container.Content = ActiveFragment)
            .ConfigureLayout(layout =>
            {
                layout.Width = width;
                layout.Height = height;
            });
    }

    private void ReallocateSurfaceIfNeeded(float scale)
    {
        int physW = (int)Math.Ceiling(ArrangedWidth * scale);
        int physH = (int)Math.Ceiling(ArrangedHeight * scale);
        EnsureSurfaceSize(physW, physH);
    }

    private void DrawToSurface(float scale, bool arrange)
    {
        var bounds = new GuiBounds(
            new GuiPoint(0, 0, IsAbsolute: true),
            new GuiSize(ArrangedWidth, ArrangedHeight));
        DrawSurfaceContents(bounds, scale, arrange);
    }

    private void Blit()
    {
        if (ActiveFragment is null)
        {
            return;
        }

        if (ArrangedWidth <= 0 || ArrangedHeight <= 0)
        {
            return;
        }

        var (posX, posY) = GetScreenPosition(PhysicalWidth, PhysicalHeight, _currentScale);
        BlitAt(posX, posY);
    }

    public override bool ContainsScreenPoint(int x, int y)
    {
        if (!IsActive || ArrangedWidth <= 0 || ArrangedHeight <= 0)
        {
            return false;
        }

        float scale = RuntimeEnv.GUIScale;
        var (posX, posY) = GetScreenPosition(PhysicalWidth, PhysicalHeight, scale);
        return x >= posX && x < posX + PhysicalWidth && y >= posY && y < posY + PhysicalHeight;
    }

    public override void AddInteractiveRegion(in InteractiveRegion region)
    {
        if (_activePlacement.InputHost is null)
        {
            return;
        }

        _activePlacement.InputHost.AddInteractiveRegion(
            region.Translated(_activePlacement.InputRegionOffsetX, _activePlacement.InputRegionOffsetY));
    }

    public override void AddKeyboardRegion(in KeyboardRegion region)
    {
        if (_activePlacement.InputHost is null)
        {
            return;
        }
        // Keyboard regions are matched by token identity, not bounds; no translation.
        _activePlacement.InputHost.AddKeyboardRegion(region);
    }

    protected virtual GuiSize ResolveLogicalSize()
    {
        if (_activePlacement.FixedLogicalSize is GuiSize fixedSize)
        {
            return fixedSize;
        }

        double? maximumWidth =
            _activePlacement.MaxLogicalWidth > 0
                ? _activePlacement.MaxLogicalWidth
                : null;
        double? maximumHeight =
            _activePlacement.MaxLogicalHeight > 0
                ? _activePlacement.MaxLogicalHeight
                : null;

        if (TreeBuilder.NodeSlots.Count == 0 || TreeBuilder.NodeSlots[0].Node is not IGuiComponent rootComponent)
        {
            return new GuiSize(0, 0);
        }

        var arrangedBounds =
            rootComponent.Arrange(
                new GuiBounds(
                    new GuiPoint(0, 0, IsAbsolute: true),
                    new GuiSize(
                        maximumWidth,
                        maximumHeight)));

        return arrangedBounds.Bounds.Size
            ?? new GuiSize(0, 0);
    }

    protected virtual (double posX, double posY) GetScreenPosition(double physW, double physH, float scale) =>
        _activePlacement.Anchor.Invoke(physW, physH, scale, _clientApi);
}
