using System;
using Vintagestory.API.Client;

namespace BitzArt.QuickSearch;

internal class QuickSearchService(ICoreClientAPI clientApi) : IDisposable
{
    private bool _isDisposed = false;
    private bool _isOn = false;

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        RegisterQuickSearchHotKey();
    }

    private void RegisterQuickSearchHotKey()
    {
        clientApi.Input.AddHotKey(ModHotKeys.QuickSearch, OnSearchPressed);
    }

    private bool OnSearchPressed(KeyCombination keys)
    {
        StartSearch();
        return true;
    }

    private void StartSearch()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        _isOn = !_isOn;
        var state = _isOn ? "ON" : "OFF";

        clientApi.ShowChatMessage($"QuickSearch is now: {state}");
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        _isDisposed = true;
    }
}
