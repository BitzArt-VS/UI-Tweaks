using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Represents a node's stable mounted position and provides its runtime services.
/// </summary>
public interface IGuiNodeSlot
{
    public ICoreClientAPI ClientApi { get; }

    public IGuiNodeSlot? Parent { get; }

    public IGuiNode Node { get; }

    public IReadOnlyList<IGuiNodeSlot> Children { get; }

    public void RequestReconcile();

    public void RequestLayout();

    public void RequestRender();

    public bool TryGetCascadingValue<T>(out T value);

    public bool TryGetCascadingValue<T>(string? name, out T value);
}
