using Cairo;
using Vintagestory.API.Common;

namespace BitzArt.VS.GUI;

/// <summary>
/// Common base class for interactive input components — currently
/// <see cref="GuiTextInput"/> and <see cref="GuiCheckbox"/>. Wires up the shared
/// boilerplate so subclasses can focus on input-specific visuals and key handling:
/// <list type="bullet">
///   <item>Resolves the ambient <see cref="FocusManager"/> from the cascade chain.</item>
///   <item>Subscribes mouse callbacks on the input's own slot; on left-click it requests
///   focus for this node and forwards the press
///   to <see cref="OnInputMouseDownAsync"/> / <see cref="OnInputClickAsync"/>.</item>
///   <item>Tracks <see cref="IsHovered"/> / <see cref="IsPressed"/> for visual feedback,
///   captures the most recent allocated bounds for hit-tests during a drag, and exposes
///   <see cref="IsFocused"/> as a convenience over the focus manager.</item>
/// </list>
/// <para>
/// Subclasses are expected to override <see cref="GuiNode.Render"/> for chrome (and may
/// register keystroke handlers from <see cref="GuiNode.ConfigureSlot"/> with
/// <c>builder.OnKeyDown</c> / <c>builder.OnKeyUp</c> / <c>builder.OnKeyPress</c>).
/// </para>
/// </summary>
public abstract class GuiInputBase : GuiComponent
{
    /// <summary>When false, mouse interactions are ignored and the input cannot receive
    /// focus. Subclasses should also dim their visuals based on this flag. Default true.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The ambient focus manager resolved from the cascade chain. Null when the
    /// input is declared outside any <see cref="GuiDialog"/> tree.</summary>
    protected FocusManager? FocusManager { get; private set; }

    /// <summary>True when this input is the currently focused node.</summary>
    protected bool IsFocused => FocusManager is { } fm && fm.IsFocused(this);

    /// <summary>True while the cursor is over the input's content area (uncaptured hover).</summary>
    protected bool IsHovered { get; private set; }

    /// <summary>True between a left-button press on the input and its matching release.</summary>
    protected bool IsPressed { get; private set; }

    /// <summary>The most recently arranged bounds, captured at the start of <see cref="Render"/>
    /// — exposed so subclasses don't need to thread bounds through their own state when
    /// reacting to mouse events that fire after arrangement.</summary>
    protected GuiBounds LastBounds { get; private set; }

    /// <inheritdoc/>
    public override void OnParametersSet()
    {
        // Cached every parameters-set so the input picks up a focus manager that becomes
        // available later (e.g. dialog reopens) without requiring a fresh component instance.
        FocusManager = GetCascadingValue<FocusManager>();
    }

    /// <inheritdoc/>
    public override void Render(Context context, GuiBounds bounds)
    {
        LastBounds = bounds;
    }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        base.ConfigureSlot(builder);
        builder
            .FocusOnMouseDown(
                this,
                args => Enabled && args.Button == EnumMouseButton.Left)
            .OnMouseDown((System.Func<GuiMouseEventArgs, ValueTask>)HandleMouseDownAsync)
            .OnMouseUp((System.Func<GuiMouseEventArgs, ValueTask>)HandleMouseUpAsync)
            .OnMouseClick((System.Func<GuiMouseEventArgs, ValueTask>)HandleMouseClickAsync)
            .OnMouseMove((System.Func<GuiMouseEventArgs, ValueTask>)HandleMouseMoveAsync)
            .OnMouseEnter((Action<GuiMouseEventArgs>)HandleMouseEnter)
            .OnMouseLeave((Action<GuiMouseEventArgs>)HandleMouseLeave)
            .OnFocusChanged((Action<bool>)HandleFocusChanged);
    }

    private async ValueTask HandleMouseDownAsync(GuiMouseEventArgs args)
    {
        if (!Enabled)
        {
            return;
        }

        if (args.Button != EnumMouseButton.Left)
        {
            return;
        }

        IsPressed = true;
        await OnInputMouseDownAsync(args);
        Slot!.RequestRender();
    }

    private async ValueTask HandleMouseUpAsync(GuiMouseEventArgs args)
    {
        bool wasPressed = IsPressed;
        IsPressed = false;

        var position = LastBounds.Position!.Value;
        var size = LastBounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        double right = position.X + width;
        double bottom = position.Y + height;

        IsHovered = args.Position.X >= position.X && args.Position.X < right
                 && args.Position.Y >= position.Y && args.Position.Y < bottom;
        if (wasPressed)
        {
            await OnInputMouseUpAsync(args);
        }

        if (wasPressed || IsHovered)
        {
            Slot!.RequestRender();
        }
    }

    private async ValueTask HandleMouseClickAsync(GuiMouseEventArgs args)
    {
        if (!Enabled)
        {
            return;
        }

        if (args.Button != EnumMouseButton.Left)
        {
            return;
        }

        await OnInputClickAsync(args);
    }

    private async ValueTask HandleMouseMoveAsync(GuiMouseEventArgs args)
    {
        if (!Enabled)
        {
            return;
        }

        if (!IsPressed)
        {
            return;
        }

        await OnInputMouseMoveAsync(args);
        Slot!.RequestRender();
    }

    private void HandleMouseEnter(GuiMouseEventArgs args)
    {
        IsHovered = true;
        Slot!.RequestRender();
    }

    private void HandleMouseLeave(GuiMouseEventArgs args)
    {
        IsHovered = false;
        Slot!.RequestRender();
    }

    private void HandleFocusChanged(bool focused) => Slot!.RequestRender();

    /// <summary>Hook invoked on left-button mouse-down inside the input. Default: no-op.</summary>
    protected virtual ValueTask OnInputMouseDownAsync(GuiMouseEventArgs e) =>
        ValueTask.CompletedTask;

    /// <summary>Hook invoked on a complete left-button click (down + up both inside the
    /// input). Default: no-op.</summary>
    protected virtual ValueTask OnInputClickAsync(GuiMouseEventArgs e) =>
        ValueTask.CompletedTask;

    /// <summary>Hook invoked on left-button mouse-up after a previous press on this input,
    /// regardless of where the cursor currently is (mouse capture). Fires before
    /// <see cref="OnInputClickAsync"/> when the release happens inside the input. Default: no-op.</summary>
    protected virtual ValueTask OnInputMouseUpAsync(GuiMouseEventArgs e) =>
        ValueTask.CompletedTask;

    /// <summary>Hook invoked on mouse movement while the input has captured the mouse —
    /// i.e. between a press and its matching release, including while the cursor is
    /// outside the input's bounds. Basis for drag interactions like slider scrubbing.
    /// Default: no-op.</summary>
    protected virtual ValueTask OnInputMouseMoveAsync(GuiMouseEventArgs e) =>
        ValueTask.CompletedTask;
}
