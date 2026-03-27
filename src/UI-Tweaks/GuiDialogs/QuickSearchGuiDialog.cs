using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace BitzArt.UI.Tweaks;

internal partial class QuickSearchGuiDialog : GuiDialog
{
    private readonly QuickSearchService _searchService;
    private readonly List<IFlatListItem> _results;
    private readonly QuickSearchResultItem _calculatorResultItem = new();

    private Action<string, bool, bool>? _setSearchText;
    private string _input = string.Empty;

    public override double DrawOrder => 0.3;

    public QuickSearchGuiDialog(ICoreClientAPI clientApi, QuickSearchService search) : base(clientApi)
    {
        _searchService = search;
        _results = [];

        RegisterQuickSearchHotKey();
        Compose();
    }

    const double _listHeight = 200;

    private void Compose()
    {
        var searchFieldBounds = ElementBounds.Fixed(0, 20, 424, 32);
        var resultListBounds = ElementBounds.Fixed(0, 60, 400, _listHeight);

        var clipBounds = resultListBounds.ForkBoundingParent();

        var scrollbarBounds = resultListBounds.CopyOffsetedSibling(resultListBounds.fixedWidth + 4).WithFixedWidth(20);

        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        SingleComposer = ClientApi.Gui
            .CreateCompo("quicksearch-dialog", ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
            .AddShadedDialogBG(bgBounds, true)
            .AddDialogTitleBar(Lang.Get($"{Constants.ModId}:quicksearch"), () => TryClose())
            .BeginChildElements(bgBounds)
                .AddTextInput(searchFieldBounds, OnTextInputChanged, key: "quick-search-input")
                .AddInset(resultListBounds)
                .BeginClip(clipBounds)
                    .AddFlatList(resultListBounds, OnItemClicked, _results, "resultList")
                .EndClip()
                .AddVerticalScrollbar(OnNewScrollbarValue, scrollbarBounds, "scrollbar")
            .EndChildElements()
            .Compose();

        var scrollbar = SingleComposer.GetScrollbar("scrollbar");
        var flatList = SingleComposer.GetFlatList("resultList");

        if (scrollbar is not null && flatList is not null)
        {
            scrollbar.SetHeights((float)_listHeight, (float)flatList.insideBounds.fixedHeight);
        }
        

        var textInput = SingleComposer.GetTextInput("quick-search-input");
        var selectedTextStartField = typeof(GuiElementEditableTextBase).GetField("selectedTextStart", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find 'selectedTextStart' field in GuiElementEditableTextBase");

        _setSearchText = (text, select, setEnd) =>
        {
            textInput.LoadValue([text]);

            if (setEnd)
            {
                if (select)
                {
                    selectedTextStartField.SetValue(textInput, 0);
                }
                textInput.SetCaretPos(text.Length);
            }
        };
    }

    protected void OnNewScrollbarValue(float value)
    {
        var stacklist = SingleComposer.GetFlatList("resultList");

        stacklist.insideBounds.fixedY = 3 - value;
        stacklist.insideBounds.CalcWorldBounds();
    }

    private void RegisterQuickSearchHotKey()
    {
        ClientApi.Input.AddHotKey(ModHotKeys.QuickSearch, (keys) =>
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (IsOpened())
            {
                TryClose();
                return true;
            }

            TryOpenOnKeyPress();

            return true;
        });
    }

    public override void OnGuiOpened()
    {
        Compose();

        _setSearchText!.Invoke(_input, true, true);
        Search();

        ClientApi.Logger.VerboseDebug($"QuickSearch is now: ON");
    }

    public override void OnGuiClosed()
    {
        ClientApi.Logger.VerboseDebug($"QuickSearch is now: OFF");
    }

    protected void Compose(bool enqueueOnMainThread)
    {
        if (enqueueOnMainThread)
        {
            ClientApi.Event.EnqueueMainThreadTask(() =>
            {
                Compose();
            }, "quicksearch-compose");

            return;
        }

        Compose();
    }

    private void OnTextInputChanged(string newText)
    {
        if (_input == newText)
        {
            return;
        }

        _input = newText;
        ClientApi.Logger.VerboseDebug($"QuickSearch input: '{newText}'");

        ClientApi.Gui.PlaySound("menubutton_press");

        Search();
    }

    private void Search()
    {
        _results.Clear();

        // Check if the input looks like a math expression before trying to evaluate it
        if (!string.IsNullOrWhiteSpace(_input) && GetMathRegex().IsMatch(_input))
        {
            try
            {
                // Replace percentage matches with their decimal equivalents before evaluating the expression.
                // (DataTable.Compute doesn't support percentages)
                var input = GetPercentageRegex().Replace(_input, match =>
                {
                    if (double.TryParse(match.Groups[1].Value, out var number))
                    {
                        return (number / 100).ToString();
                    }
                    return match.Value; // If parsing fails, return the original match
                });

                // Quick and dirty way to evaluate simple math expressions without writing a custom parser.
                // Requires a try-catch to function though, which is unfortunate.
                // A custom parser would be more versatile and efficient.
                var result = new DataTable().Compute(input, null);

                _calculatorResultItem.Text = $"={result}";
                _results.Add(_calculatorResultItem);

                return;
            }
            catch
            {
            }
        }

        SearchItems();
    }

    private void SearchItems()
    {
        _results.AddRange(_searchService.Search(_input));

        var resultList = SingleComposer.GetFlatList("resultList");
        resultList.CalcTotalHeight();

        var scrollbar = SingleComposer.GetScrollbar("scrollbar");
        scrollbar.SetHeights((float)_listHeight, (float)resultList.insideBounds.fixedHeight);

        return;
    }

    public void OnItemClicked(int index)
    {
        Task.Run(() =>
        {
            var item = (QuickSearchResultItem)_results.ElementAtOrDefault(index)!;
            if (item.OnClicked(ClientApi))
            {
                ClientApi.Event.EnqueueMainThreadTask(() =>
                {
                    TryClose();
                }, "quicksearch-close-on-item-clicked");
            }
        });
    }

    public override void OnMouseDown(MouseEvent args)
    {
        if (!IsInDialog(args))
        {
            TryClose();
            args.Handled = true;
        }

        base.OnMouseDown(args);
    }

    public override void OnMouseUp(MouseEvent args)
    {
        if (!IsInDialog(args))
        {
            TryClose();
            args.Handled = true;
        }

        base.OnMouseUp(args);
    }

    private bool IsInDialog(MouseEvent args)
    {
        var dialogBounds = SingleComposer.Bounds;

        return args.X >= dialogBounds.absX
            && args.X <= dialogBounds.absX + dialogBounds.OuterWidth
            && args.Y >= dialogBounds.absY
            && args.Y <= dialogBounds.absY + dialogBounds.OuterHeight;
    }

    [GeneratedRegex("^(\\d+|[+\\-*\\/^()%,.]|\\s)+$")]
    private static partial Regex GetMathRegex();

    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex GetPercentageRegex();
}
