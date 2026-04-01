using BitzArt.UI.Tweaks.Services;
using Cairo;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class HudTooltipLabel : HudElement
{
    protected GameStatusService StatusService { get; private init; }
    protected CairoFont Font { get; private init; }
    protected string Format { get; private init; }

    protected EnumDialogArea DialogArea;
    protected (double X, double Y) Offset;
    protected double Width = 200.0;
    protected double Height = 18.0;

    protected bool HasBackground;
    protected double BackgroundOpacity;

    private const string _richtextElementName = "tooltip-value";

    private readonly string? _runtimeFormat;

    public HudTooltipLabel(ICoreClientAPI clientApi, GameStatusService statusService, IHudTooltipConfiguration config)
        : base(clientApi)
    {
        StatusService = statusService;

        DialogArea = config.Area;
        Offset = (config.X, config.Y);
        Width = config.Width;

        HasBackground = config.HasBackground;
        BackgroundOpacity = config.BackgroundOpacity;

        Font = new CairoFont
        {
            Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
            Fontname = GuiStyle.StandardFontName,
            UnscaledFontsize = GuiStyle.DetailFontSize - 1
        };
        Format = config.Format;

        _runtimeFormat = config is IFormatReplacements replacementsConfig
            ? replacementsConfig.Replace(Format)
            : Format;

        if (!config.Enable)
        {
            return;
        }

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, Offset.X, Offset.Y)
            .WithFixedSize(Width, Height);

        var backgroundBoundary = ElementBounds.Fixed(0, 0, Width, Height);

        SingleComposer = clientApi.Gui
            .CreateCompo(config.ComponentName, componentBoundary)
            .AddIf(HasBackground)
                .AddTooltipBackground(backgroundBoundary, BackgroundOpacity)
            .EndIf()
            .AddRichtext(string.Empty, Font, ElementBounds.Fixed(0, 1, Width, Height - 1), _richtextElementName)
            .Compose();

        TryOpen();

        StatusService.Subscribe(_runtimeFormat, OnStatsUpdate, out _runtimeFormat);
    }

    private void OnStatsUpdate(object[] values)
    {
        var valueElement = SingleComposer.GetRichtext(_richtextElementName);

        var value = string.Format(_runtimeFormat!, [.. values]);

        ClientApi.Event.EnqueueMainThreadTask(() =>
        {
            valueElement.SetNewText($"<font align=center>{value}</font>", Font);
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
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, opacity);

            // Extract coordinates and dimensions
            double x = Bounds.drawX;
            double y = Bounds.drawY;
            double w = Bounds.OuterWidth;
            double h = Bounds.OuterHeight;
            double r = 16.0;

            // Prevent the radius from being larger than half the width/height
            r = Math.Min(r, Math.Min(w / 2, h / 2));

            // Build the rounded rectangle path
            ctx.NewPath();
            ctx.Arc(x + w - r, y + r, r, -Math.PI / 2, 0);          // Top-Right corner
            ctx.Arc(x + w - r, y + h - r, r, 0, Math.PI / 2);       // Bottom-Right corner
            ctx.Arc(x + r, y + h - r, r, Math.PI / 2, Math.PI);     // Bottom-Left corner
            ctx.Arc(x + r, y + r, r, Math.PI, 3 * Math.PI / 2);     // Top-Left corner
            ctx.ClosePath(); // Connects the last arc back to the first

            // Fill the path with the RGBA source set above
            ctx.Fill();
        }
    }
}
