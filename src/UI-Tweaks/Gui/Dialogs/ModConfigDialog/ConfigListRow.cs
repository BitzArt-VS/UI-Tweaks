using BitzArt.UI.Tweaks.Gui;
using Cairo;

namespace BitzArt.UI.Tweaks;

internal sealed class ConfigListRow : GuiComponent
{
    private const double TextLeftPadding = 14;
    private const double DescriptionTopGap = 3;
    private const double SeparatorHeight = 1;

    private static readonly GuiColor HoverBackground = GuiColor.FromRgba(0.48, 0.38, 0.27, 0.14);
    private static readonly GuiColor PressedBackground = GuiColor.FromRgba(0, 0, 0, 0.22);
    private static readonly GuiColor SeparatorColor = GuiColor.FromRgba(0.78, 0.69, 0.58, 0.16);
    private static readonly GuiColor ActiveTextColor = GuiColor.FromRgba(0.96, 0.92, 0.86, 1.0);
    private static readonly GuiColor DescriptionTextColor = GuiColor.FromRgba(0.78, 0.70, 0.60, 0.92);

    public string Text { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GuiCallback OnClick { get; set; }

    private bool _isHovered;
    private bool _isPressed;

    protected override GuiBounds ResolveFinalBounds(
        GuiBounds availableBounds,
        GuiBounds? descendantsBounds)
    {
        GuiSize textSize = GuiFontStyle.MediumBold.Measure(Text);
        double measuredWidth =
            (textSize.Width ?? 0) + TextLeftPadding * 2;
        double measuredHeight =
            textSize.Height ?? 0;

        if (!string.IsNullOrEmpty(Description))
        {
            GuiSize descriptionSize =
                GuiFontStyle.Small.Measure(Description);

            measuredWidth =
                Math.Max(
                    textSize.Width ?? 0,
                    descriptionSize.Width ?? 0)
                + TextLeftPadding * 2;

            measuredHeight =
                (textSize.Height ?? 0)
                + DescriptionTopGap
                + (descriptionSize.Height ?? 0);
        }

        var measuredSize = new GuiSize(
            Math.Max(0, measuredWidth),
            Math.Max(0, measuredHeight));

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

        if (_isHovered)
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
            Color = _isHovered ? ActiveTextColor : GuiVanillaStyle.ButtonTextColor
        };
        GuiSize textSize = font.Measure(Text);
        double textHeight = textSize.Height ?? 0;

        if (string.IsNullOrEmpty(Description))
        {
            double textY = position.Y + (height - textHeight) / 2.0;
            context.DrawText(Text, font, position.X + TextLeftPadding, textY);
        }
        else
        {
            var descriptionFont = GuiFontStyle.Small with { Color = DescriptionTextColor };
            GuiSize descriptionSize = descriptionFont.Measure(Description);
            double descriptionHeight = descriptionSize.Height ?? 0;
            double blockHeight = textHeight + DescriptionTopGap + descriptionHeight;
            double titleY = position.Y + (height - blockHeight) / 2.0;
            double descriptionY = titleY + textHeight + DescriptionTopGap;

            context.DrawText(Text, font, position.X + TextLeftPadding, titleY);
            context.DrawText(Description, descriptionFont, position.X + TextLeftPadding, descriptionY);
        }

        context.Rectangle(position.X, position.Y + height - SeparatorHeight, width, SeparatorHeight);
        context.FillSolid(SeparatorColor);
    }

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        builder
            .OnMouseDown(HandleMouseDown)
            .OnMouseUp(HandleMouseUp)
            .OnMouseClick(HandleMouseClick)
            .OnMouseEnter(HandleMouseEnter)
            .OnMouseLeave(HandleMouseLeave);
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

    private void HandleMouseClick(GuiMouseEventArgs args)
    {
        OnClick.Invoke();
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
