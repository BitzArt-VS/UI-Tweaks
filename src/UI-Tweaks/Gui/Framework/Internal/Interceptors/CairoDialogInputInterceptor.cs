using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks.Gui;

internal sealed class CairoDialogInputInterceptor(ICoreClientAPI clientApi, DialogRenderer renderer)
    : VanillaGuiDialog(clientApi)
{
    private readonly DialogRenderer _renderer = renderer;

    // Forward the Cairo renderer's render order as the vanilla DrawOrder so this dialog
    // stacks correctly within game.OpenedGuis. GuiManager.OnRenderFrameGUI iterates that
    // list in reverse, calling OnRenderGUI on each, and RequestFocus() shuffles the
    // focused dialog to the front of its DrawOrder rank — which means it is rendered last
    // (on top) within the rank without us having to re-register a renderer.
    public override double DrawOrder => _renderer.RenderOrder;

    public override void OnGuiOpened() { }

    // Drive the Cairo render from inside vanilla's per-dialog Ortho pass so this dialog
    // shares the z-stack with vanilla dialogs (instead of all of them painting on top of
    // us via GuiManager's single Ortho-1.0 renderer slot).
    public override void OnRenderGUI(float deltaTime) => _renderer.OnRenderFrame(deltaTime, EnumRenderStage.Ortho);

    public override void Focus()
    {
        base.Focus();
        _renderer.OnFocus();
    }

    public override void UnFocus()
    {
        base.UnFocus();
        _renderer.OnUnFocus();
    }

    public override void OnMouseDown(MouseEvent args) => _renderer.OnMouseDown(args);
    public override void OnMouseUp(MouseEvent args) => _renderer.OnMouseUp(args);
    public override void OnMouseMove(MouseEvent args) => _renderer.OnMouseMove(args);
    public override void OnMouseWheel(MouseWheelEventArgs args) => _renderer.OnMouseWheel(args);

    public override void OnKeyDown(KeyEvent args) => _renderer.OnKeyDown(args);
    public override void OnKeyPress(KeyEvent args) => _renderer.OnKeyPress(args);
    public override void OnKeyUp(KeyEvent args) => _renderer.OnKeyUp(args);
    public override bool OnEscapePressed() => _renderer.OnEscapePressed();
}
