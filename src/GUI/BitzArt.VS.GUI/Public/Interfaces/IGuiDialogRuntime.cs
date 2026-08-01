namespace BitzArt.VS.GUI;

public interface IGuiDialogRuntime
{
    IGuiNode? FocusedNode { get; }

    void RequestClose();

    void RequestFocus();

    void SetFocusedNode(IGuiNode? node);

    void SetMouseOverCursor(string? cursor);
}
