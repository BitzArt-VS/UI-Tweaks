using BitzArt.UI.Tweaks.Services;
using Cairo;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class HudTooltipLabel : HudElement
{
    private const string RichtextElementName = "tooltip-text";

    protected EnumDialogArea DialogArea;
    protected (double X, double Y) Offset;
    protected double Width;
    protected double Height;
    protected bool CenterText;

    protected bool HasBackground;
    protected double BackgroundOpacity;
    protected double BackgroundCornerRadius;

    protected List<string> FormatStrings;

    protected GameStatusService StatusService { get; private init; }
    protected CairoFont Font { get; private init; }

    public HudTooltipLabel(ICoreClientAPI clientApi, GameStatusService statusService, IHudTooltipConfiguration config)
        : base(clientApi)
    {
        StatusService = statusService;

        DialogArea = config.Area;
        Offset = (config.Offset.X, config.Offset.Y);
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

        FormatStrings = [config.Format, .. config.ExtraElements ?? []];

        if (!config.Enable)
        {
            return;
        }

        var componentBoundary = ElementBounds
            .FixedPos(DialogArea, Offset.X, Offset.Y)
            .WithFixedSize(Width, Height);

        var backgroundBoundary = ElementBounds.Fixed(0, 0, Width, Height);

        SingleComposer = clientApi.Gui
            .CreateCompo(config.ComponentName, componentBoundary)
            .AddIf(HasBackground)
                .AddTooltipBackground(backgroundBoundary, BackgroundOpacity)
            .EndIf();

        for (int i = 0; i < FormatStrings.Count; i++)
        {
            var contentBoundary = ElementBounds.Fixed(
                config.Padding.Left,
                config.Padding.Top,
                Width - (config.Padding.Left + config.Padding.Right),
                Height - (config.Padding.Top + config.Padding.Bottom));

            SingleComposer = SingleComposer
                .AddRichtext(string.Empty, Font, contentBoundary, $"{RichtextElementName}-{i + 1}");
        }

        SingleComposer = SingleComposer.Compose();

        TryOpen();

        for (int i = 0; i < FormatStrings.Count; i++)
        {
            var index = i; // Capture loop variable for closure
            var format = FormatStrings[index];

            if (!StatusService.Subscribe(format, (value) => OnStatsUpdate(value, index)))
            {
                // No subscription created, likely no variable placeholders found in the format string.
                // Still need to update the text once with the static format.
                OnStatsUpdate(format, index);
            }
        }
    }

    private void OnStatsUpdate(string? value, int index)
    {
        var valueElement = SingleComposer.GetRichtext($"{RichtextElementName}-{index + 1}");
        var format = FormatStrings[index];

        if (CenterText)
        {
            value = $"<font align=center>{value}</font>";
        }

        ClientApi.Event.EnqueueMainThreadTask(() =>
        {
            valueElement.SetNewText(value, Font);
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
