using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

internal sealed class RenderHandle(
    GuiSurfaceRenderer renderer,
    GuiRenderTreeBuilder childBuilder,
    GuiRenderTreeBuilder? parentBuilder) : IGuiRenderHandle
{
    private readonly GuiSurfaceRenderer _renderer = renderer;
    private readonly GuiRenderTreeBuilder _childBuilder = childBuilder;
    private readonly GuiRenderTreeBuilder? _parentBuilder = parentBuilder;

    public ICoreClientAPI ClientApi => _renderer.ClientApi;

    public void RequestReconcile(GuiRenderFragment renderFragment)
        => _renderer.Schedule(renderFragment, _childBuilder);

    public void RequestArrange()
        => _renderer.RequestArrange();

    public void RequestPaint()
        => _renderer.RequestPaint();

    public void RequestRender()
        => RequestPaint();

    public bool TryGetCascadingValue<T>(out T value)
        => TryGetCascadingValue(name: null, out value);

    public bool TryGetCascadingValue<T>(string? name, out T value)
    {
        // Read the parent builder's chain on every call — it is the live reference the
        // grand-parent reconcile updates. _parentBuilder is null only for the root dialog
        // (no ancestor providers possible), so the short-circuit returns "not found".
        var chain = _parentBuilder?.CascadeChain;
        if (chain is null)
        {
            value = default!;
            return false;
        }
        return chain.TryGet(name, out value);
    }
}
