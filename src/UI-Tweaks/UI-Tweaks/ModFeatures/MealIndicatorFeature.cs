using BitzArt.UI.Tweaks.Config;
using HarmonyLib;
using System.ComponentModel;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

public sealed class MealIndicatorFeature(
    UiTweaksModSystem modSystem,
    GameTweaksConfig config)
    : ModSystemFeature<UiTweaksModSystem, GameTweaksConfig>(modSystem, config)
{
    private const string HarmonyId = $"{Constants.ModId}.meal-remaining-indicator";

    private ICoreClientAPI? _clientApi;
    private Harmony? _harmony;
    private bool _isPatchApplied;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void Start(ICoreClientAPI clientApi)
    {
        _clientApi = clientApi;
        _harmony = new Harmony(HarmonyId);
        Config.PropertyChanged += OnConfigPropertyChanged;

        ApplyPatchState();
    }

    public override void Dispose()
    {
        Config.PropertyChanged -= OnConfigPropertyChanged;
        RemovePatch();

        _harmony = null;
        _clientApi = null;

        GC.SuppressFinalize(this);
    }

    private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        ApplyPatchState();
    }

    private void ApplyPatchState()
    {
        if (Config.ShowMealRemainingIndicator)
        {
            AddPatch();
            return;
        }

        RemovePatch();
    }

    private void AddPatch()
    {
        if (_harmony is null || _isPatchApplied)
        {
            return;
        }

        MealIndicatorPatch.Patch(_harmony);
        _isPatchApplied = true;
        RefreshInventoryOverlays();
    }

    private void RemovePatch()
    {
        if (_harmony is null || !_isPatchApplied)
        {
            return;
        }

        MealIndicatorPatch.Unpatch(_harmony);
        _isPatchApplied = false;
        RefreshInventoryOverlays();
    }

    private void RefreshInventoryOverlays()
    {
        var inventories = _clientApi?.World.Player?.InventoryManager.Inventories.Values;
        if (inventories is null)
        {
            return;
        }

        foreach (var inventory in inventories)
        {
            for (int slotIndex = 0; slotIndex < inventory.Count; slotIndex++)
            {
                inventory.DirtySlots.Add(slotIndex);
            }
        }
    }
}
