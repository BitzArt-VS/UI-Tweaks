using Cairo;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

public interface IGuiNode
{
    public GuiRenderFragment RenderFragment { get; }

    public void Attach(IGuiRenderHandle renderHandle, ICoreClientAPI clientApi);

    public void OnInitialized() { }

    public void OnParametersSet() { }

    /// <summary>
    /// Called once per frame while this node is focused. Use for local time-based state
    /// such as caret blinking.
    /// </summary>
    public void OnFrame(float deltaTime) { }

    /// <summary>
    /// Called each frame to draw this node within the given bounds. Save and restore
    /// Cairo context state around any transforms.
    /// </summary>
    public void Render(Context context, GuiComponentBounds bounds) { }

    /// <summary>
    /// Called after all children have rendered, before the next sibling slot draws.
    /// An overlay can still be obscured by a later sibling that overlaps the same area.
    /// Save and restore Cairo context state around any transforms.
    /// </summary>
    public void RenderOverlay(Context context, GuiComponentBounds bounds) { }

}
