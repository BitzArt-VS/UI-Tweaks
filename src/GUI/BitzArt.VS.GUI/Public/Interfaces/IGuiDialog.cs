namespace BitzArt.VS.GUI;

public interface IGuiDialog : IGuiComponent
{
    /// <summary>
    /// Horizontal offset from screen-centre in logical pixels before display scaling.
    /// Drives dialog dragging.
    /// </summary>
    double OffsetX { get; }

    /// <summary>
    /// Vertical offset from screen-centre in logical pixels before display scaling.
    /// Drives dialog dragging.
    /// </summary>
    double OffsetY { get; }

    void AttachDialogRuntime(IGuiDialogRuntime runtime) { }

}
