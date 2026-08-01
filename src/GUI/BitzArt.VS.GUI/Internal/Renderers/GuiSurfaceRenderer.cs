using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.VS.GUI;

internal abstract class GuiSurfaceRenderer : IDisposable
{
    protected readonly ICoreClientAPI _clientApi;
    private ImageSurface? _surface;
    private Context? _context;
    private LoadedTexture _texture;
    private int _physicalWidth;
    private int _physicalHeight;
    private readonly ClippingContext _clippingContext = new();
    protected float _currentScale;
    protected bool _reconcileRequested;
    protected bool _arrangeRequested;
    protected bool _renderRequested;

    public ICoreClientAPI ClientApi => _clientApi;

    protected int PhysicalWidth => _physicalWidth;
    protected int PhysicalHeight => _physicalHeight;
    protected GuiTreeBuilder TreeBuilder { get; }
    protected bool HasPendingSurfaceUpdate => _reconcileRequested || _arrangeRequested || _renderRequested;
    internal ClippingContext ClippingContext => _clippingContext;

    protected GuiSurfaceRenderer(ICoreClientAPI clientApi)
    {
        _clientApi = clientApi;
        _texture = new LoadedTexture(clientApi);
        _currentScale = RuntimeEnv.GUIScale;
        TreeBuilder = new GuiTreeBuilder(this);
    }

    protected void RequestReconcile()
    {
        _reconcileRequested = true;
        RequestArrange();
    }

    public void RequestArrange()
    {
        _arrangeRequested = true;
        RequestRender();
    }

    public void RequestRender() => _renderRequested = true;

    public virtual void Schedule(GuiTreeFragment fragment, GuiTreeBuilder builder) => RequestReconcile();
    public virtual void Cancel(GuiTreeFragment fragment) { }

    public virtual void AddInteractiveRegion(in InteractiveRegion region) { }
    public virtual void AddResizeRegion(in ResizeRegion region) { }
    public virtual void AddKeyboardRegion(in KeyboardRegion region) { }

    public virtual bool ContainsScreenPoint(int x, int y) => false;

    internal void SetCascadeChain(CascadingValueChain? chain)
    {
        TreeBuilder.CascadeChain = new CascadingValueChain(
            chain,
            typeof(ClippingContext),
            name: null,
            _clippingContext);
    }

    protected void EnsureSurfaceSize(int physW, int physH)
    {
        if (_surface is not null && physW == _physicalWidth && physH == _physicalHeight)
        {
            return;
        }

        _context?.Dispose();
        _surface?.Dispose();
        _surface = new ImageSurface(Format.Argb32, physW, physH);
        _context = new Context(_surface);
        _physicalWidth = physW;
        _physicalHeight = physH;
    }

    protected void DrawSurfaceContents(GuiBounds bounds, float scale, bool arrange)
    {
        DrawSurfaceContents(bounds, scale, arrange, context =>
        {
            if (arrange)
            {
                _clippingContext.Reset();
                TreeBuilder.ArrangeRoot(bounds);
            }

            TreeBuilder.Paint(context, registerRegions: arrange);
        });
    }

    private void DrawSurfaceContents(GuiBounds bounds, float scale, bool arrange, Action<Context> draw)
    {
        ClearInvalidationRequests(arrange);
        _context!.IdentityMatrix();
        _context.Operator = Operator.Source;
        _context.SetSourceRGBA(0, 0, 0, 0);
        _context.Paint();
        _context.Operator = Operator.Over;
        _context.Scale(scale, scale);
        draw(_context);
        _surface!.Flush();
        _clientApi.Gui.LoadOrUpdateCairoTexture(_surface, true, ref _texture);
        _currentScale = scale;
    }

    protected void ClearInvalidationRequests(bool arranged)
    {
        if (arranged)
        {
            _reconcileRequested = false;
            _arrangeRequested = false;
        }
        _renderRequested = false;
    }

    protected void BlitAt(double posX, double posY)
    {
        if (_texture.TextureId == 0)
        {
            return;
        }

        _clientApi.Render.Render2DTexturePremultipliedAlpha(
            _texture.TextureId, posX, posY, _physicalWidth, _physicalHeight);
    }

    public virtual void Dispose()
    {
        TreeBuilder.Dispose();
        _texture.Dispose();
        _context?.Dispose();
        _surface?.Dispose();
    }
}
