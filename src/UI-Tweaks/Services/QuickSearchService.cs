using System;
using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

internal class QuickSearchService(ICoreClientAPI clientApi) : IDisposable
{
    private bool _isDisposed = false;
    private readonly QuickSearchDialog _dialog = new(clientApi);

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        RegisterQuickSearchHotKey();
    }

    private void RegisterQuickSearchHotKey()
    {
        clientApi.Input.AddHotKey(ModHotKeys.QuickSearch, (keys) =>
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_dialog.IsOpened())
            {
                _dialog.TryClose();
                return true;
            }

            _dialog.ignoreNextKeyPress = true;
            _dialog.TryOpen();

            return true;
        });
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _dialog.Dispose();

        if (!clientApi.Input.HotKeys.Remove(ModHotKeys.QuickSearch.Code))
        {
            throw new InvalidOperationException($"Failed to unregister hotkey with code '{ModHotKeys.QuickSearch.Code}'");
        }

        _isDisposed = true;
    }
}
