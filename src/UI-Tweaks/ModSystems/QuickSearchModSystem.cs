using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

// This ModSystem initializes QuickSearch when the client starts,
// subscribing to necessary events allowing it to establish necessary item search indexes.
public class QuickSearchModSystem : ClientModSystem
{
    private QuickSearchGuiDialog? _dialog;

    protected override string Name => $"{Constants.ModName}:QuickSearch";

    protected override void Start(ICoreClientAPI api)
    {
        _dialog = new(api, new(api));
    }

    public override void Dispose()
    {
        _dialog?.Dispose();
        _dialog = null;
    }
}
