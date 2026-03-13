using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace BitzArt.UI.Tweaks;

internal class QuickSearchDialog : ModGuiDialog
{
    private string _input = string.Empty;

    private Action<string, bool, bool>? _setSearchText;
    List<ItemStack>? _items = [];

    public override double DrawOrder => 0.3;

    public QuickSearchDialog(ICoreClientAPI clientApi) : base(clientApi)
    {
        clientApi.Event.LevelFinalize += () =>
        {
            _items = [.. ClientApi.World.Collectibles
                .SelectMany(collectible => collectible.GetHandBookStacks(ClientApi) ?? [])];
        };
        

        Compose();
    }

    private void Compose()
    {
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        SingleComposer = ClientApi.Gui
        .CreateCompo("quicksearch-dialog", ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
        .AddGrayBG(bgBounds)
        .BeginChildElements(bgBounds)
        .AddTextInput(ElementBounds.Fixed(0, 16, 280, 32), OnTextInputChanged, CairoFont.TextInput(), "quick-search-input")
        .AddDynamicText(string.Empty, CairoFont.WhiteDetailText(), ElementBounds.Fixed(0, 64, 200, 64), "resultText")
        .Compose();

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

    public override void OnGuiOpened()
    {
        Compose();

        _setSearchText!.Invoke(_input, true, true);

        ClientApi.Logger.VerboseDebug($"QuickSearch is now: ON");
    }

    public override void OnGuiClosed()
    {
        ClientApi.Logger.VerboseDebug($"QuickSearch is now: OFF");
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

        _ = SearchAsync();
    }

    private Task SearchAsync()
    {
        SetResultText(string.Empty);

        if (string.IsNullOrEmpty(_input))
        {
            return Task.CompletedTask;
        }

        if (_items is null)
        {
            return Task.CompletedTask;
        }

        // Temporary implementation, for testing purposes
        // TODO: Extract search logic to a separate service and implement proper searching with inverse indexing
        var resultItems = _items.Where(x => x.GetName().ToSearchFriendly().Contains(_input, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToArray();

        ClientApi.Event.EnqueueMainThreadTask(() =>
        {
            SetResultText(string.Join(", ", resultItems.Select(x => x.GetName())));
        }, "quicksearch-set-results");

        return Task.CompletedTask;
    }

    private void SetResultText(string text)
    {
        SingleComposer.GetDynamicText("resultText")?.SetNewText(text);
    }
}
