using BitzArt.UI.Tweaks.Services;
using Cairo;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class HudTooltipLabel : HudElement
{
    protected GameStatusService StatusService { get; private init; }
    protected CairoFont Font { get; private init; }
    protected string Format { get; private set; }

    protected EnumDialogArea DialogArea;
    protected (double X, double Y) Offset;
    protected double Width;
    protected double Height;
    protected bool CenterText;

    protected bool HasBackground;
    protected double BackgroundOpacity;
    protected double BackgroundCornerRadius;

    private const string _richtextElementName = "tooltip-value";

    public HudTooltipLabel(ICoreClientAPI clientApi, GameStatusService statusService, IHudTooltipConfiguration config)
        : base(clientApi)
    {
        StatusService = statusService;

        DialogArea = config.Area;
        Offset = (config.X, config.Y);
        Height = config.Height;
        Width = config.Width;
        CenterText = config.CenterText;

        HasBackground = config.HasBackground;
        BackgroundOpacity = config.BackgroundOpacity;
        BackgroundCornerRadius = config.BackgroundCornerRadius;

        Font = new CairoFont
        {
            Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
            Fontname = GuiStyle.DecorativeFontName,
            UnscaledFontsize = config.FontSize
        };
        Format = config.Format;

        if (!config.Enable)
        {
            return;
        }

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, Offset.X, Offset.Y)
            .WithFixedSize(Width, Height);

        var backgroundBoundary = ElementBounds.Fixed(0, 0, Width, Height);

        var contentPadding = BackgroundCornerRadius * 0.75;

        var contentBoundary = ElementBounds.Fixed(contentPadding, 0, Width - (contentPadding * 2), Height);

        SingleComposer = clientApi.Gui
            .CreateCompo(config.ComponentName, componentBoundary)
            .AddIf(HasBackground)
                .AddTooltipBackground(backgroundBoundary, BackgroundOpacity)
            .EndIf()
            .AddRichtext(string.Empty, Font, contentBoundary, _richtextElementName)
            .Compose();

        TryOpen();

        StatusService.Subscribe(Format, OnStatsUpdate, out var format);
        Format = format;
    }

    private void OnStatsUpdate(object[] values)
    {
        var valueElement = SingleComposer.GetRichtext(_richtextElementName);

        var text = string.Format(Format!, [.. values]);

        if (CenterText)
        {
            text = $"<font align=center>{text}</font>";
        }

        ClientApi.Event.EnqueueMainThreadTask(() =>
        {
            valueElement.SetNewText(text, Font);
        }, "ui-tweaks-tooltip-value-update");
    }
}

file static class BackgroundExtensions
{
    public static GuiComposer AddTooltipBackground(this GuiComposer composer, ElementBounds bounds, double backgroundOpacity)
    {
        if (!composer.Composed)
        {
            composer.AddStaticElement(new TooltipBackgroundElement(composer.Api, bounds, backgroundOpacity));
        }

        return composer;
    }

    private class TooltipBackgroundElement(ICoreClientAPI capi, ElementBounds bounds, double opacity)
        : GuiElement(capi, bounds)
    {
        public override void ComposeElements(Context ctx, ImageSurface surface)
        {
            Bounds.CalcWorldBounds();

            // 1. Cast to int (or use Math.Floor) to match how the engine allocates texture bounds
            double x = (int)Bounds.drawX;
            double y = (int)Bounds.drawY;
            double w = (int)Bounds.OuterWidth;
            double h = (int)Bounds.OuterHeight;

            // 2. Setup border properties
            double lineWidth = 2.0;
            double inset = lineWidth / 2.0; // 1.0px inset for a 2.0px line

            // 3. Apply the inset PLUS a 1-pixel safety buffer on the right/bottom
            x += inset;
            y += inset;
            w -= (inset * 2) + 1.0;
            h -= (inset * 2) + 1.0;

            double r = 16.0;
            r = Math.Min(r, Math.Min(w / 2, h / 2));

            // ... (The rest of your drawing code stays exactly the same)
            ctx.NewPath();
            ctx.Arc(x + w - r, y + r, r, -Math.PI / 2, 0);
            ctx.Arc(x + w - r, y + h - r, r, 0, Math.PI / 2);
            ctx.Arc(x + r, y + h - r, r, Math.PI / 2, Math.PI);
            ctx.Arc(x + r, y + r, r, Math.PI, 3 * Math.PI / 2);
            ctx.ClosePath();

            ctx.SetSourceRGBA(0.12, 0.11, 0.10, opacity);
            ctx.FillPreserve();

            double borderOpacity = 1.0 - ((1.0 - opacity) / 2.0);
            ctx.SetSourceRGBA(0.35, 0.33, 0.30, borderOpacity);
            ctx.LineWidth = lineWidth;
            ctx.Stroke();
        }
    }
}
