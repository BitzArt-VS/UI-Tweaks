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

    public GuiComponentBounds? Bounds { get; }

    /// <summary>
    /// Ensures the mounted component has an arrangement result when its layout
    /// dependencies can be resolved.
    /// </summary>
    /// <param name="layoutChanged">
    /// Whether layout inputs changed since the cached arrangement.
    /// </param>
    /// <returns>
    /// The cached or newly calculated bounds. Individual bounds fields are
    /// <c>null</c> while their geometry is unresolved.
    /// </returns>
    public GuiComponentBounds Arrange(bool layoutChanged = false);

    public void RequestReconcile();

    public void RequestLayout();

    public void RequestRender();

    public bool TryGetCascadingValue<T>(out T value);

    public bool TryGetCascadingValue<T>(string? name, out T value);
}
