using BitzArt.UI.Tweaks.Config;
using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

public class QuickSearchModSystem : ClientModSystem
{
    private QuickSearchGuiDialog? _quickSearchDialog;

    protected override string Name => $"{Constants.ModName}:QuickSearch";

    public override void Dispose()
    {
        _quickSearchDialog?.Dispose();
        _quickSearchDialog = null;
    }

    protected override void Start(ICoreClientAPI clientApi)
    {
        var config = clientApi.GetModConfig<UiTweaksModConfig>(Constants.ModConfigFileName);

        clientApi.Input.AddHotKey(ModHotKeys.ModConfiguration, (keys) =>
        {
            OpenConfigDialog(clientApi);
            return true;
        });

        if (config.QuickSearch.Enable)
        {
            _quickSearchDialog = new(clientApi, new(clientApi), config);

            clientApi.Input.AddHotKey(ModHotKeys.QuickSearch, (keys) =>
            {
                if (_quickSearchDialog is null)
                {
                    throw new NullReferenceException("QuickSearch dialog somehow turned out to not be initialized.");
                }

                if (_quickSearchDialog.IsOpened())
                {
                    _quickSearchDialog.TryClose();
                    return true;
                }

                _quickSearchDialog.TryOpenOnKeyPress();

                return true;
            });
        }
    }

    private static void OpenConfigDialog(ICoreClientAPI clientApi)
    {
        // TODO: Mod config dialog

        clientApi.ShowChatMessage("Mod configuration dialog is to be implemented.");
    }
}
