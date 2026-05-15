namespace BitzArt.UI.Tweaks.Gui;

/// <summary>
/// Layout-transparent wrapper used as an explicit mouse-event target around visible
/// content. Attach slot-level <c>OnMouse*</c> builder extensions to the target and provide
/// the wrapped subtree through <see cref="Content"/>.
/// </summary>
public sealed class GuiMouseTarget : GuiNode
{
    public GuiRenderFragment? Content { get; set; }

    protected override void BuildRenderTree(IGuiRenderTreeBuilder builder)
    {
        Content?.Invoke(builder);
    }
}
