using BitzArt.UI.Tweaks.Config;
using Cairo;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace BitzArt.UI.Tweaks;

internal class ModConfigGuiDialog : GuiDialog
{
    private const string DialogComposerKey = "mod-config-dialog";
    private const string ScrollbarKey = "content-scrollbar";
    private const int DialogContentWidth = 400;
    private const int ContentAreaHeight = 400;
    private const int ContentPaddingX = 8;
    private const int ScrollbarWidth = 20;
    private const int BreadcrumbHeight = 28;
    private const int BreadcrumbLineGap = 4;
    private const int BreadcrumbSeparatorWidth = 36;
    private const int BreadcrumbContentGap = 12;
    private const int SaveDebounceMs = 10000;

    private readonly CairoFont _breadcrumbFont;

    private readonly UiTweaksModConfig _config;
    private readonly List<ConfigPage> _pageStack;
    private CancellationTokenSource? _saveDebounce;
    private readonly Lock _saveDebounceLock = new();
    private ElementBounds? _contentScrollBounds;

    public override double DrawOrder => 0.2;

    public ModConfigGuiDialog(ICoreClientAPI clientApi, UiTweaksModConfig config) : base(clientApi)
    {
        _config = config;
        _pageStack = [new RootConfigPage(_config)];

        _breadcrumbFont = new()
        {
            Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
            Fontname = GuiStyle.StandardFontName,
            UnscaledFontsize = GuiStyle.SmallFontSize,
            FontWeight = FontWeight.Bold
        };

        Compose();
    }

    public override void OnGuiOpened() => Compose();

    public override void Dispose()
    {
        _breadcrumbFont.Dispose();

        if (_saveDebounce is not null)
        {
            lock (_saveDebounceLock)
            {
                _saveDebounce.Cancel();
                _saveDebounce.Dispose();
            }
            
            _saveDebounce = null;

            ClientApi.StoreModConfig(_config, Constants.ModConfigFileName);
        }

        base.Dispose();
    }

    private void PushPage(ConfigPage page)
    {
        _pageStack.Add(page);
        Compose();
    }

    private void PopToPage(int index)
    {
        _pageStack.RemoveRange(index + 1, _pageStack.Count - index - 1);
        Compose();
    }

    private void Compose()
    {
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        var composer = ClientApi.Gui
            .CreateCompo(DialogComposerKey, ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
            .AddShadedDialogBG(bgBounds, true)
            .AddDialogTitleBar(Lang.Get($"{Constants.ModId}:ui-tweaks-config"), () => TryClose())
            .BeginChildElements(bgBounds);

        double contentY = 24;
        int viewportHeight;

        if (_pageStack.Count > 1)
        {
            int breadcrumbHeight = CalculateBreadcrumbHeight();
            ComposeBreadcrumbs(composer, contentY);
            contentY += breadcrumbHeight + BreadcrumbContentGap;
            viewportHeight = ContentAreaHeight + BreadcrumbHeight - breadcrumbHeight;
        }
        else
        {
            viewportHeight = ContentAreaHeight + BreadcrumbHeight + BreadcrumbContentGap;
        }

        _contentScrollBounds = ElementBounds.Fixed(0, 0, DialogContentWidth, 0);
        _contentScrollBounds.BothSizing = ElementSizing.FitToChildren;

        var viewportBounds = ElementBounds.Fixed(0, contentY, DialogContentWidth, viewportHeight);
        var clipBounds = viewportBounds.ForkBoundingParent();
        var scrollbarBounds = ElementBounds.Fixed(DialogContentWidth + 4, contentY, ScrollbarWidth, viewportHeight);
        var localBounds = ElementBounds.Fixed(ContentPaddingX, 0, DialogContentWidth - ContentPaddingX * 2, 0);
        localBounds.BothSizing = ElementSizing.FitToChildren;

        composer
            .AddInset(viewportBounds)
            .BeginClip(clipBounds)
            .BeginChildElements(_contentScrollBounds);

        double contentHeight = _pageStack[^1].ComposeContent(ClientApi, composer, localBounds, LaunchSaveConfig, PushPage);

        composer
            .EndChildElements()
            .EndClip()
            .AddVerticalScrollbar(OnScrollbarValueChanged, scrollbarBounds, ScrollbarKey);

        SingleComposer = composer
            .EndChildElements()
            .Compose();

        _pageStack[^1].OnComposed(SingleComposer);

        var scrollbar = SingleComposer.GetScrollbar(ScrollbarKey);
        scrollbar?.SetHeights(viewportHeight, (float)contentHeight);
    }

    private void OnScrollbarValueChanged(float value)
    {
        if (_contentScrollBounds == null)
        {
            return;
        }
        
        _contentScrollBounds.fixedY = -value;
        _contentScrollBounds.CalcWorldBounds();
    }

    private void LaunchSaveConfig()
    {
        Task.Run(async () =>
        {
            if (_saveDebounce is not null)
            {
                lock (_saveDebounceLock)
                {
                    _saveDebounce.Cancel();
                    _saveDebounce.Dispose();
                }
            }
            
            _saveDebounce = new CancellationTokenSource();

            await Task.Delay(SaveDebounceMs, _saveDebounce.Token);
            ClientApi.StoreModConfig(_config, Constants.ModConfigFileName);

            _saveDebounce = null;
        });
    }

    private int CalculateBreadcrumbHeight()
    {
        int lines = 1;
        double xOffset = 0;

        for (int i = 0; i < _pageStack.Count - 1; i++)
        {
            double buttonWidth = _pageStack[i].Title.Length * 8 + 32;

            if (xOffset > 0 && xOffset + buttonWidth > DialogContentWidth)
            {
                lines++;
                xOffset = 0;
            }

            xOffset += buttonWidth + BreadcrumbSeparatorWidth;
        }

        double labelWidth = _pageStack[^1].Title.Length * 8 + 32;
        if (xOffset > 0 && xOffset + labelWidth > DialogContentWidth)
        {
            lines++;
        }

        return lines * BreadcrumbHeight + (lines - 1) * BreadcrumbLineGap;
    }

    private void ComposeBreadcrumbs(GuiComposer composer, double yOffset)
    {
        double xOffset = 0;
        int line = 0;

        for (int i = 0; i < _pageStack.Count - 1; i++)
        {
            int capturedIndex = i;
            double buttonWidth = _pageStack[i].Title.Length * 8 + 32;

            if (xOffset > 0 && xOffset + buttonWidth > DialogContentWidth)
            {
                line++;
                xOffset = 0;
            }

            double lineY = yOffset + line * (BreadcrumbHeight + BreadcrumbLineGap);

            composer.AddSmallButton(
                _pageStack[i].Title,
                () => { PopToPage(capturedIndex); return true; },
                ElementBounds.Fixed(xOffset, lineY, buttonWidth, BreadcrumbHeight),
                key: $"breadcrumb-{i}");

            composer.AddStaticText(
                ">>",
                CairoFont.WhiteSmallText(),
                ElementBounds.Fixed(xOffset + buttonWidth + 10, lineY + (BreadcrumbHeight - 14) / 2.0, 20, 14));

            xOffset += buttonWidth + BreadcrumbSeparatorWidth;
        }

        double labelWidth = _pageStack[^1].Title.Length * 8 + 32;
        if (xOffset > 0 && xOffset + labelWidth > DialogContentWidth)
        {
            line++;
            xOffset = 0;
        }

        double labelLineY = yOffset + line * (BreadcrumbHeight + BreadcrumbLineGap);

        composer.AddStaticText(
            _pageStack[^1].Title,
            _breadcrumbFont,
            ElementBounds.Fixed(xOffset, labelLineY + (BreadcrumbHeight - 14) / 2.0, DialogContentWidth - xOffset, 14));
    }
}
