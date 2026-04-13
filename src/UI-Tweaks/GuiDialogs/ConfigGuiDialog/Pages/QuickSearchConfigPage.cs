using BitzArt.UI.Tweaks.Config;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal sealed class QuickSearchConfigPage(QuickSearchConfig config) : ConfigPage(Lang.Get($"{Constants.ModId}:config-page-quicksearch"))
{
    private const string EnableKey = "qs-enable";
    private const string ResultListHeightKey = "qs-result-list-height";

    private const int RowHeight = 30;
    private const int RowGap = 6;
    private const int ControlWidth = 160;
    private const int SwitchSize = 20;
    private const int SliderHeight = 20;

    private const int MinResultListHeight = 50;
    private const int MaxResultListHeight = 600;
    private const int ResultListHeightStep = 10;

    private readonly QuickSearchConfig _config = config;

    public override void ComposeContent(ICoreClientAPI clientApi, GuiComposer composer, ElementBounds bounds, Action saveConfig, Action<ConfigPage> pushPage)
    {
        double x = bounds.fixedX;
        double y = bounds.fixedY;
        double labelWidth = bounds.fixedWidth - ControlWidth;

        var enableLabelBounds = ElementBounds.Fixed(x, y, labelWidth, RowHeight);
        var enableSwitchBounds = ElementBounds.Fixed(x + labelWidth, y + (RowHeight - SwitchSize) / 2.0, SwitchSize, SwitchSize);

        double row1Y = y + RowHeight + RowGap;

        var heightLabelBounds = ElementBounds.Fixed(x, row1Y, labelWidth, RowHeight);
        var heightSliderBounds = ElementBounds.Fixed(x + labelWidth, row1Y + (RowHeight - SliderHeight) / 2.0, ControlWidth, SliderHeight);

        composer
            .AddStaticText(Lang.Get($"{Constants.ModId}:config-quicksearch-enable"), CairoFont.WhiteSmallText(), enableLabelBounds)
            .AddSwitch(val => { _config.Enable = val; saveConfig(); }, enableSwitchBounds, EnableKey)
            .AddStaticText(Lang.Get($"{Constants.ModId}:config-quicksearch-result-list-height"), CairoFont.WhiteSmallText(), heightLabelBounds)
            .AddSlider(val => { _config.ResultListHeight = val; saveConfig(); return true; }, heightSliderBounds, ResultListHeightKey);
    }

    public override void OnComposed(GuiComposer composer)
    {
        composer.GetSwitch(EnableKey).SetValue(_config.Enable);
        composer.GetSlider(ResultListHeightKey).SetValues(_config.ResultListHeight, MinResultListHeight, MaxResultListHeight, ResultListHeightStep);
    }
}
