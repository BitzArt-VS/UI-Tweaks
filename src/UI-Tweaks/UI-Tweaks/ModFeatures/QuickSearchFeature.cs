using BitzArt.UI.Tweaks.Config;
using System.ComponentModel;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

public class QuickSearchFeature(UiTweaksModSystem modSystem, QuickSearchConfig config)
    : ModSystemFeature<UiTweaksModSystem, QuickSearchConfig>(modSystem, config)
{
    private IInputAPI? _inputApi;
    private QuickSearchGuiDialog? _dialog;
    private bool _isHotKeyRegistered;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void Start(ICoreClientAPI clientApi)
    {
        _inputApi = clientApi.Input;
        _dialog = new(clientApi, new(clientApi), Config);

        Config.PropertyChanged += OnConfigPropertyChanged;
        UpdateHotKeyRegistration();
    }

    public override void Dispose()
    {
        Config.PropertyChanged -= OnConfigPropertyChanged;

        if (_inputApi is not null)
        {
            ModHotKeys.QuickSearch.Unregister(_inputApi);
            _isHotKeyRegistered = false;
            _inputApi = null;
        }

        _dialog?.Dispose();
        _dialog = null;

        GC.SuppressFinalize(this);
    }

    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        UpdateHotKeyRegistration();
    }

    private void UpdateHotKeyRegistration()
    {
        if (Config.Enable)
        {
            if (_isHotKeyRegistered)
            {
                return;
            }

            _inputApi!.AddHotKey(ModHotKeys.QuickSearch, keys => ToggleDialog());
            _isHotKeyRegistered = true;
            return;
        }

        ModHotKeys.QuickSearch.Unregister(_inputApi!);
        _isHotKeyRegistered = false;
        _dialog?.TryClose();
    }

    private bool ToggleDialog()
    {
        if (_dialog is null)
        {
            throw new NullReferenceException("QuickSearch dialog is not initialized.");
        }

        if (_dialog.IsOpened())
        {
            _dialog.TryClose();
            return true;
        }

        _dialog.TryOpenOnKeyPress();

        return true;
    }
}
