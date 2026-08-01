using Cairo;
using Vintagestory.API.Client;

namespace BitzArt.VS.GUI;

/// <summary>
/// Two-state checkbox (vanilla calls these "switches"). Visually shares the embossed dark
/// recessed panel of <see cref="GuiTextInput"/>; when <see cref="Checked"/> the centre of
/// the panel is filled with a solid square mark. Click toggles, and while focused
/// <c>Space</c> / <c>Enter</c> also toggles — matching vanilla <c>GuiElementSwitch</c>.
/// </summary>
public sealed class GuiCheckbox : GuiInputBase
{
    /// <summary>Current toggle state. Setting this directly does not fire
    /// <see cref="OnCheckedChanged"/>; use <see cref="ToggleAsync"/> or click the checkbox to
    /// raise the event.</summary>
    public bool Checked { get; set; }

    /// <summary>Side length of the checkbox in logical pixels. Default 24 — slightly
    /// smaller than vanilla's 30, which fits modern dialogs better while keeping the same
    /// proportions.</summary>
    public double Size { get; set; } = 24;

    /// <summary>Padding between the chrome and the inner mark, in logical pixels.</summary>
    public double Padding { get; set; } = 4;

    /// <summary>Fill colour of the inner check mark when <see cref="Checked"/>. Defaults
    /// to <see cref="GuiStyle.SwitchMarkColor"/> — the approximation of what vanilla's
    /// black + water-pattern fill looks like without the pattern texture.</summary>
    public GuiColor MarkColor { get; set; } = GuiVanillaStyle.SwitchMarkColor;

    /// <summary>Fired after <see cref="Checked"/> changes due to user interaction.</summary>
    public GuiCallback<bool>? OnCheckedChanged { get; set; }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        base.ConfigureSlot(builder);
        builder.OnKeyDown((Func<GuiKeyEventArgs, ValueTask>)HandleKeyDownAsync);
    }

    /// <inheritdoc/>
    protected override GuiComponentBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
    {
        double measuredSize = Math.Max(0, Size);

        return LayoutParameters.ResolveBounds(
            availableBounds,
            new GuiBounds(
                null,
                new GuiSize(measuredSize, measuredSize)));
    }

    /// <summary>Flips <see cref="Checked"/>, fires <see cref="OnCheckedChanged"/>, and
    /// plays the vanilla toggle sound. No-op when <see cref="GuiInputBase.Enabled"/> is false.</summary>
    public async ValueTask ToggleAsync()
    {
        if (!Enabled)
        {
            return;
        }

        Checked = !Checked;
        ClientApi?.Gui.PlaySound("toggleswitch");
        Slot!.RequestRender();

        if (OnCheckedChanged is GuiCallback<bool> onCheckedChanged)
        {
            await onCheckedChanged.InvokeAsync(Checked);
        }
    }

    protected override ValueTask OnInputClickAsync(GuiMouseEventArgs e) => ToggleAsync();

    private async ValueTask HandleKeyDownAsync(GuiKeyEventArgs args)
    {
        if (!Enabled || !IsFocused)
        {
            return;
        }

        if (args.KeyCode == (int)GlKeys.Space || args.KeyCode == (int)GlKeys.Enter)
        {
            args.Handled = true;
            await ToggleAsync();
            return;
        }
        // Swallow other keys so global hotkeys don't fire while the checkbox is focused
        // (e.g. typing a hotkey letter wouldn't otherwise be consumed by anything else).
        // Tab / Escape stay unhandled so dialog-level traversal / close still work.
        if (args.KeyCode == (int)GlKeys.Tab || args.KeyCode == (int)GlKeys.Escape)
        {
            return;
        }

        args.Handled = true;
    }

    /// <inheritdoc/>
    public override void Render(Context ctx, GuiBounds bounds)
    {
        base.Render(ctx, bounds);

        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        // 1. Emboss chrome — vanilla switch uses depth=1 (shallower than the text input's
        //    depth=2); the 20% black brightness overlay is identical across both. Using
        //    depth=1 here keeps the checkbox visually lighter and matches vanilla.
        GuiInset.Draw(ctx, bounds, depth: 1, brightness: 0.8f, radius: 1);

        // 2. Hover wash only — vanilla's switch shows no focus indicator, and a sticky
        //    focus highlight on a one-shot toggle reads as "the control is still active"
        //    long after the user has clicked away mentally. Keyboard users still get
        //    feedback because Space / Enter visibly flips the mark.
        if (Enabled && IsHovered)
        {
            ctx.Rectangle(position.X, position.Y, width, height);
            ctx.SetSourceRGBA(1, 1, 1, 0.05);
            ctx.Fill();
        }

        // 3. Check mark — a solid coloured square inset by Padding on every side. Mirrors
        //    vanilla's "fill the inner area when On" behaviour without the water pattern
        //    (consistent with other framework components that drop vanilla's pattern fills).
        if (Checked)
        {
            double mx = position.X + Padding;
            double my = position.Y + Padding;
            double mw = Math.Max(0, width - 2 * Padding);
            double mh = Math.Max(0, height - 2 * Padding);
            ctx.RoundRect(mx, my, mw, mh, 1);
            var c = MarkColor;
            ctx.SetSourceRGBA(c.R, c.G, c.B, c.A * (Enabled ? 1.0 : 0.45));
            ctx.Fill();
        }
    }
}
