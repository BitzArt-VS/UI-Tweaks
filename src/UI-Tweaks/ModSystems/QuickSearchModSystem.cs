using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BitzArt.UI.Tweaks;

// ModSystems serve as entrypoints for code mods.
// This ModSystem initializes QuickSearch when the client starts,
// and disposes of it when the ModSystem is unloaded.
public class QuickSearchModSystem : ModSystem
{
    private QuickSearchDialog? _dialog;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        _dialog = new(api, new(api));
    }

    public override void Dispose()
    {
        _dialog?.Dispose();
    }
}
