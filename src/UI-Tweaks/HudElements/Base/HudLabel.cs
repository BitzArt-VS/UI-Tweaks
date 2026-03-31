using BitzArt.UI.Tweaks.Services;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public abstract class HudLabel : HudElement
{
    protected GameStatusService StatusService { get; private init; }
    protected CairoFont Font { get; private init; }
    protected string Format { get; private init; }

    protected IEnumerable<FormatReplacement>? FormatReplacements = null;
    protected EnumDialogArea DialogArea = EnumDialogArea.CenterMiddle;
    protected (double X, double Y) Offset = (0, 0);
    protected double Width = 400.0;
    protected double Height = 20.0;

    private const string _richtextElementName = "tooltip-value";

    private readonly string? _runtimeFormat;

    protected record struct FormatReplacement(string Placeholder, string RecordKey);

    public HudLabel(ICoreClientAPI clientApi, GameStatusService statusService, IHudTooltipConfiguration config)
        : base(clientApi)
    {
        StatusService = statusService;
        Font = CairoFont.WhiteDetailText();
        Offset = (config.X, config.Y);
        Format = config.Format;

        OnInitialize();

        _runtimeFormat = ReplaceCustomFormatPlaceholders(Format);

        if (!config.Enable)
        {
            return;
        }

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, Offset.X, Offset.Y)
            .WithFixedSize(Width, Height);

        SingleComposer = clientApi.Gui
            .CreateCompo(config.ComponentName, componentBoundary)
            .AddRichtext(string.Empty, Font, ElementBounds.Fixed(0, 0, Width, Height), _richtextElementName)
            .Compose();

        TryOpen();

        StatusService.Subscribe(_runtimeFormat, OnStatsUpdate, out _runtimeFormat);
    }

    protected virtual void OnInitialize() { }

    private string ReplaceCustomFormatPlaceholders(string format)
    {
        if (FormatReplacements is null)
        {
            return format;
        }

        foreach (var (placeholder, recordKey) in FormatReplacements)
        {
            format = format.Replace($"{{{placeholder}}}", $"{{{recordKey}}}", StringComparison.OrdinalIgnoreCase);
        }

        return format;
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
