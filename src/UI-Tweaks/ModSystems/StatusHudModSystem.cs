using Vintagestory.API.Client;

namespace BitzArt.UI.Tweaks;

// This ModSystem initializes the StatusHud functionality when the client starts,
// and disposes of it when the ModSystem is unloaded.
public class StatusHudModSystem : ClientModSystem
{
    protected override string Name => $"{Constants.ModName}:StatusHUD";

    protected override void Start(ICoreClientAPI clientApi)
    {
        // TODO: Implement StatusHUD ModSystem.
    }
}
