using BitzArt.VS.GUI;
using Cairo;

namespace BitzArt.UI.Tweaks;

internal sealed class ConfigNavigationRow : GuiComponent
{
    private const double TextLeftPadding = 14;
    private const double AccentWidth = 3;
    private const double SeparatorHeight = 1;

    private static readonly GuiColor SelectedBackground = GuiColor.FromRgba(0.38, 0.29, 0.20, 0.58);
    private static readonly GuiColor HoverBackground = GuiColor.FromRgba(0.48, 0.38, 0.27, 0.14);
    private static readonly GuiColor PressedBackground = GuiColor.FromRgba(0, 0, 0, 0.22);
    private static readonly GuiColor SeparatorColor = GuiColor.FromRgba(0.78, 0.69, 0.58, 0.16);
    private static readonly GuiColor AccentColor = GuiVanillaStyle.ActiveButtonTextColor;
    private static readonly GuiColor ActiveTextColor = GuiColor.FromRgba(0.96, 0.92, 0.86, 1.0);

    public string Text { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public GuiCallback? OnClick { get; set; }

    private bool _isHovered;
    private bool _isPressed;

    protected override GuiComponentBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
    {
        GuiSize textSize = GuiFontStyle.MediumBold.Measure(Text);
        var measuredSize = new GuiSize(
            Math.Max(
                0,
                (textSize.Width ?? 0) + TextLeftPadding * 2),
            Math.Max(0, textSize.Height ?? 0));

        return LayoutParameters.ResolveBounds(
            availableBounds,
            new GuiBounds(null, measuredSize));
    }

    public override void Render(Context context, GuiBounds bounds)
    {
        var position = bounds.Position!.Value;
        var size = bounds.Size!.Value;
        double width = size.Width!.Value;
        double height = size.Height!.Value;

        if (IsSelected)
        {
            context.Rectangle(position.X, position.Y, width, height);
            context.FillSolid(SelectedBackground);

            context.Rectangle(position.X, position.Y, AccentWidth, height);
            context.FillSolid(AccentColor);
        }
        else if (_isHovered)
        {
            context.Rectangle(position.X, position.Y, width, height);
            context.FillSolid(HoverBackground);
        }

        if (_isPressed)
        {
            context.Rectangle(position.X, position.Y, width, height);
            context.FillSolid(PressedBackground);
        }

        var font = GuiFontStyle.MediumBold with
        {
            Color = IsSelected || _isHovered
                ? ActiveTextColor
                : GuiVanillaStyle.ButtonTextColor
        };
        GuiSize textSize = font.Measure(Text);
        double textY = position.Y + (height - (textSize.Height ?? 0)) / 2.0;
        context.DrawText(Text, font, position.X + TextLeftPadding, textY);

        context.Rectangle(position.X, position.Y + height - SeparatorHeight, width, SeparatorHeight);
        context.FillSolid(SeparatorColor);
    }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        builder
            .OnMouseDown((Action<GuiMouseEventArgs>)HandleMouseDown)
            .OnMouseUp((Action<GuiMouseEventArgs>)HandleMouseUp)
            .OnMouseClick((Func<GuiMouseEventArgs, ValueTask>)HandleMouseClickAsync)
            .OnMouseEnter((Action<GuiMouseEventArgs>)HandleMouseEnter)
            .OnMouseLeave((Action<GuiMouseEventArgs>)HandleMouseLeave);
    }

    private void HandleMouseDown(GuiMouseEventArgs args)
    {
        _isPressed = true;
        ClientApi?.Gui.PlaySound("menubutton_down");
        Slot!.RequestRender();
    }

    private void HandleMouseUp(GuiMouseEventArgs args)
    {
        _isPressed = false;
        Slot!.RequestRender();
    }

    private async ValueTask HandleMouseClickAsync(GuiMouseEventArgs args)
    {
        if (OnClick is GuiCallback onClick)
        {
            await onClick.InvokeAsync();
        }
    }

    private void HandleMouseEnter(GuiMouseEventArgs args)
    {
        _isHovered = true;
        ClientApi?.Gui.PlaySound("menubutton");
        Slot!.RequestRender();
    }

    private void HandleMouseLeave(GuiMouseEventArgs args)
    {
        _isHovered = false;
        Slot!.RequestRender();
    }
}
