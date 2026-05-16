using BitzArt.UI.Tweaks.Config;
using BitzArt.UI.Tweaks.Gui;
using Cairo;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal sealed class ZoomModConfigPage : GuiComponent, IModConfigPage
{
    public static string PageName => Lang.Get($"{Constants.ModId}:config-page-zoom");

    private const double LabelColumnWidth = 224;
    private const double RowHeight = 28;
    private const double RowSpacing = 8;

    private static readonly GuiFontStyle ItalicNoteFont = GuiFontStyle.Default with
    {
        Slant = FontSlant.Italic,
    };

    private ModConfigContext? _context;

    protected override void ConfigureSlot(IGuiSlotBuilder builder)
    {
        builder.ConfigureLayout(layout => layout.Padding = new(8));
    }

    public override void OnParametersSet()
    {
        _context = GetCascadingValue<ModConfigContext>();
    }

    protected override void BuildRenderTree(IGuiRenderTreeBuilder builder)
    {
        var config = _context?.Config.Zoom;

        if (config is null)
        {
            return;
        }

        BuildSettingRow(builder, key: 1,
            label: Lang.Get($"{Constants.ModId}:config-zoom-enable"),
            tooltip: builder =>
            {
                builder.AddLabel(0, Lang.Get($"{Constants.ModId}:config-zoom-enable-tooltip"));
                builder.AddLabel(1, string.Empty);
                builder.AddLabel(2, Lang.Get($"{Constants.ModId}:config-requires-restart"),
                    font: ItalicNoteFont,
                    margin: new(0, 0, GuiVanillaStyle.HalfPadding, 0));
            },
            control: builder => builder.AddCheckbox(0,
                checked_: config.Enable,
                onCheckedChanged: value =>
                {
                    config.Enable = value;
                    config.NotifyPropertyChanged(nameof(ZoomConfig.Enable));
                    _context!.SaveConfig();
                }));

        BuildSettingRow(builder, key: 2,
            label: Lang.Get($"{Constants.ModId}:config-zoom-strength"),
            tooltip: builder => builder.AddLabel(0, Lang.Get($"{Constants.ModId}:config-zoom-strength-tooltip")),
            control: builder => builder.AddSlider(0,
                value: ClampSliderValue(config.Strength, 1, 10),
                minValue: 1,
                maxValue: 10,
                step: 1,
                triggerOnMouseUp: true,
                widthMode: GuiSizeMode.Fill,
                onValueChanged: value =>
                {
                    config.Strength = value;
                    config.NotifyPropertyChanged(nameof(ZoomConfig.Strength));
                    _context!.SaveConfig();
                }));

        BuildSettingRow(builder, key: 3,
            label: Lang.Get($"{Constants.ModId}:config-zoom-speed"),
            tooltip: builder => builder.AddLabel(0, Lang.Get($"{Constants.ModId}:config-zoom-speed-tooltip")),
            control: builder => builder.AddSlider(0,
                value: ClampSliderValue(config.Speed, 1, 10),
                minValue: 1,
                maxValue: 10,
                step: 1,
                triggerOnMouseUp: true,
                widthMode: GuiSizeMode.Fill,
                onValueChanged: value =>
                {
                    config.Speed = value;
                    config.NotifyPropertyChanged(nameof(ZoomConfig.Speed));
                    _context!.SaveConfig();
                }));

        BuildSettingRow(builder, key: 4,
            label: Lang.Get($"{Constants.ModId}:config-zoom-vignette-strength"),
            tooltip: builder => builder.AddLabel(0, Lang.Get($"{Constants.ModId}:config-zoom-vignette-strength-tooltip")),
            control: builder => builder.AddSlider(0,
                value: ClampSliderValue(config.VignetteStrength, 0, 10),
                minValue: 0,
                maxValue: 10,
                step: 1,
                triggerOnMouseUp: true,
                widthMode: GuiSizeMode.Fill,
                onValueChanged: value =>
                {
                    config.VignetteStrength = value;
                    config.NotifyPropertyChanged(nameof(ZoomConfig.VignetteStrength));
                    _context!.SaveConfig();
                }));
    }

    private static int ClampSliderValue(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static void BuildSettingRow(
        IGuiRenderTreeBuilder builder,
        int key,
        string label,
        GuiRenderFragment tooltip,
        GuiRenderFragment control)
    {
        builder.AddContainer(key,
            widthMode: GuiSizeMode.Fill,
            height: RowHeight,
            direction: GuiDirection.Horizontal,
            margin: new(0, 0, RowSpacing, 0),
            content: builder =>
            {
                builder.AddTooltip(0,
                    tooltip: tooltip,
                    content: builder => builder.AddLabel(0, label,
                        width: LabelColumnWidth,
                        verticalAlignment: GuiVerticalAlignment.Center));

                builder.AddContainer(1, fill: true, content: control);
            });
    }
}
