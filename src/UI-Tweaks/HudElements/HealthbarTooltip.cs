using System;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal class HealthbarTooltip : HudElement
{
    private readonly UiTweaksModConfig.HudConfig.TooltipOptions _config;
    private readonly CairoFont _font = CairoFont.WhiteDetailText();

    private long? _tickListenerId;

    private float? _currentHealth;
    private float? _maxHealth;

    public HealthbarTooltip(ICoreClientAPI clientApi, UiTweaksModConfig.HudConfig.TooltipOptions config)
        : base(clientApi)
    {
        _config = config;

        var componentBoundary = ElementBounds
            .FixedPos(EnumDialogArea.CenterBottom, -252.0 + _config.Offset.X, -83.0 + _config.Offset.Y)
            .WithFixedSize(200.0, 20.0);

        SingleComposer = clientApi.Gui
            .CreateCompo("healthbar-tooltip", componentBoundary)
            .AddRichtext(string.Empty, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 0, 200.0, 20.0), "value")
            .Compose();

        _tickListenerId = clientApi.Event.RegisterGameTickListener(OnGameTick, 100);

        TryOpen();
    }

    private void OnGameTick(float _)
    {
        Task.Run(UpdateValues);
    }

    private void UpdateValues()
    {
        var healthAttribute = ClientApi.World?.Player?.Entity?.WatchedAttributes?.GetTreeAttribute("health");

        if (healthAttribute is null)
        {
            return;
        }

        var current = healthAttribute.TryGetFloat("currenthealth");
        var max = healthAttribute.TryGetFloat("maxhealth");

        if (current is null || max is null || current == _currentHealth && max == _maxHealth)
        {
            return;
        }

        _currentHealth = current;
        _maxHealth = max;

        var percentage = Math.Round((double)(max > 0 ? current / max * 100 : 0), 1);

        var format = _config.Format;
        var value = string.Format(format, [Math.Round(current.Value, 1), Math.Round(max.Value, 1), percentage]);

        var textComponent = SingleComposer.GetRichtext("value");
        
        ClientApi.Event.EnqueueMainThreadTask(() =>
        {
            textComponent.SetNewText($"<font align=center>{value}</font>", _font);
        }, $"{Constants.ModId}-healthbar-tooltip-update");
    }

    public override void Dispose()
    {
        if (_tickListenerId is not null)
        {
            ClientApi.Event.UnregisterGameTickListener(_tickListenerId!.Value);
            _tickListenerId = null;
        }

        base.Dispose();
    }
}
