using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

internal class SatietyTooltip : HudElement
{
    private readonly UiTweaksModConfig.HudConfig.TooltipOptions _config;
    private readonly CairoFont _font = CairoFont.WhiteDetailText();

    private static readonly string[][] _formatReplacements =
    [
        ["current", "curr", "cur", "c"],
        ["maximum", "max", "m", "total"],
        ["percentage", "percent", "p", "%"],
        ["hunger", "h"]
    ];
    private readonly string _format;
    private readonly bool _usePercentage;

    private long? _tickListenerId;
    private EntityPlayer? _playerEntity;

    public SatietyTooltip(ICoreClientAPI clientApi, UiTweaksModConfig.HudConfig.TooltipOptions config)
        : base(clientApi)
    {
        _config = config;
        _format = _config.Format;

        for (int i = 0; i < _formatReplacements.Length; i++)
        {
            foreach (var placeholder in _formatReplacements[i])
            {
                _format = _format.Replace($"{{{placeholder}}}", $"{{{i}}}");
            }
        }

        _usePercentage = _format.Contains("{2}");

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, 252.0 + _config.Offset.X, -83.0 + _config.Offset.Y)
            .WithFixedSize(400.0, 20.0);

        SingleComposer = clientApi.Gui
            .CreateCompo("satiety-tooltip", componentBoundary)
            .AddRichtext(string.Empty, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 0, 400.0, 20.0), "value")
            .Compose();

        _tickListenerId = clientApi.Event.RegisterGameTickListener(OnGameTick, 100);

        TryOpen();
    }

    private void OnGameTick(float _)
    {
        _playerEntity = ClientApi.World?.Player?.Entity;

        if (_playerEntity?.WatchedAttributes is null)
        {
            return;
        }

        _playerEntity.WatchedAttributes.RegisterModifiedListener("hunger", UpdateStats);
        _playerEntity.WatchedAttributes.RegisterModifiedListener("stats", UpdateStats);
        _playerEntity.WatchedAttributes.RegisterModifiedListener("bodyTemp", UpdateStats);

        ClientApi.Event.UnregisterGameTickListener(_tickListenerId!.Value);
        _tickListenerId = null;
    }

    private void UpdateStats()
    {
        if (_playerEntity?.WatchedAttributes is null)
        {
            return;
        }

        var hungerAttribute = _playerEntity.WatchedAttributes.GetTreeAttribute("hunger");

        if (hungerAttribute is null)
        {
            return;
        }

        var current = hungerAttribute.TryGetFloat("currentsaturation");
        var max = hungerAttribute.TryGetFloat("maxsaturation");

        if (current is null || max is null)
        {
            return;
        }

        var percentage = _usePercentage && current is not null && max is not null
            ? max > 0 ? current / max * 100 : 0
            : 0;

        var hungerRate = _playerEntity.Stats.GetBlended("hungerrate") * 100;

        var value = string.Format(_format, [
            Math.Round(current!.Value, 1),
            Math.Round(max!.Value, 1),
            Math.Round(percentage!.Value, 1),
            Math.Round(hungerRate, 0)]);

        SingleComposer
            .GetRichtext("value")
            .SetNewText($"<font align=center>{value}</font>", _font);
    }

    public override void Dispose()
    {
        if (_tickListenerId is not null)
        {
            ClientApi.Event.UnregisterGameTickListener(_tickListenerId!.Value);
            _tickListenerId = null;
        }

        _playerEntity?.WatchedAttributes?.UnregisterListener(UpdateStats);

        base.Dispose();
    }
}
