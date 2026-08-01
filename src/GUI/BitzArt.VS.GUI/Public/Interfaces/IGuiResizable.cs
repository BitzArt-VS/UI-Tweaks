namespace BitzArt.VS.GUI;

public interface IGuiResizable : IGuiComponent
{
    /// <summary>
    /// The resize edges this object currently accepts. Return <see cref="GuiEdge.None"/>
    /// while resizing should be disabled.
    /// </summary>
    public GuiEdge SupportedResizeEdges { get; }

    /// <summary>
    /// Applies framework-suggested outer bounds for this resizable.
    /// </summary>
    public void Resize(GuiBounds bounds);
}
