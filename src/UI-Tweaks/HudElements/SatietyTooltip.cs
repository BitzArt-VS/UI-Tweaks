using BitzArt.UI.Tweaks.Services;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal class SatietyTooltip : HudElement
{
    private readonly GameStatusService _statusService;
    private readonly UiTweaksModConfig.HudConfig.TooltipOptions _config;
    private readonly CairoFont _font = CairoFont.WhiteDetailText();

    private static readonly List<(string, string)> _formatReplacements =
    [
        ("current", "player-satiety-current"),
        ("maximum", "player-satiety-max"),
        ("percent", "player-satiety-percent"),
        ("hunger", "player-satiety-hunger")
    ];

    private readonly string _format;

    public SatietyTooltip(ICoreClientAPI clientApi, GameStatusService statusService, UiTweaksModConfig.HudConfig.TooltipOptions config)
        : base(clientApi)
    {
        _config = config;
        _statusService = statusService;
        _format = _config.Format;

        if (!config.Enable)
        {
            return;
        }

        foreach (var (placeholder, recordKey) in _formatReplacements)
        {
            _format = _format.Replace($"{{{placeholder}}}", $"{{{recordKey}}}", StringComparison.OrdinalIgnoreCase);
        }

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, 252.0 + _config.Offset.X, -83.0 + _config.Offset.Y)
            .WithFixedSize(400.0, 20.0);

        SingleComposer = clientApi.Gui
            .CreateCompo("satiety-tooltip", componentBoundary)
            .AddRichtext(string.Empty, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 0, 400.0, 20.0), "value")
            .Compose();

        TryOpen();

        _statusService.Subscribe(_format, OnStatsUpdate, out _format);
    }

    private void OnStatsUpdate(object[] values)
    {
        SingleComposer
            .GetRichtext("value")
            .SetNewText($"<font align=center>{string.Format(_format, [.. values])}</font>", _font);
    }
}
