using System;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace BitzArt.UI.Tweaks;

/// <summary>
/// A <see cref="ModSystem"/> which only starts client-side.
/// </summary>
public abstract class ClientModSystem : ModSystem
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    protected sealed override void Start(ICoreServerAPI serverApi)
        => throw new NotSupportedException($"The mod system '{Name}' does not support starting server-side.");
}

/// <summary>
/// A <see cref="ModSystem"/> which only starts server-side.
/// </summary>
public abstract class ServerModSystem : ModSystem
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    protected sealed override void Start(ICoreClientAPI clientApi)
        => throw new NotSupportedException($"The mod system '{Name}' does not support starting client-side.");
}

/// <summary>
/// A custom, safer, and more robust base class for ModSystems,
/// extending the normal <see cref="Vintagestory.API.Common.ModSystem"/>.
/// </summary>
/// <remarks>
/// ModSystems serve as entrypoints for code mods,
/// allowing the developer to run necessary code when the game starts,
/// both client and server-side, and interact with the game engine through the provided APIs.
/// </remarks>
public abstract partial class ModSystem : Vintagestory.API.Common.ModSystem
{
    protected virtual string Name => GetModSystemNamePartRegex().Replace(GetType().Name, string.Empty);

    private string GetStartErrorMessage(string side)
        => $"An error occurred while starting the mod system '{Name}' ({side}). The system will not start.";

    public sealed override void StartClientSide(ICoreClientAPI clientApi)
    {
        base.StartClientSide(clientApi);
        try
        {
            Start(clientApi);
        }
        catch (Exception ex)
        {
            clientApi.Logger.Error(ex);
            clientApi.Logger.Error(GetStartErrorMessage("client-side"));

            OnStartFailed(clientApi, ex);
            Dispose();
        }
    }

    public sealed override void StartServerSide(ICoreServerAPI serverApi)
    {
        base.StartServerSide(serverApi);
        try
        {
            Start(serverApi);
        }
        catch (Exception ex)
        {
            serverApi.Logger.Error(ex);
            serverApi.Logger.Error(GetStartErrorMessage("server-side"));

            OnStartFailed(serverApi, ex);
            Dispose();
        }
    }

    /// <summary>
    /// Starts the mod system client-side.
    /// </summary>
    /// <param name="clientApi">API for interacting with game engine client-side.</param>
    protected abstract void Start(ICoreClientAPI clientApi);

    /// <summary>
    /// A method invoked when an exception is thrown during attempting to start the mod system client-side.
    /// </summary>
    /// <param name="clientApi">API for interacting with game engine client-side.</param>
    /// <param name="ex">The exception thrown during attempting to start the mod system.</param>
    protected virtual void OnStartFailed(ICoreClientAPI clientApi, Exception ex) { }

    /// <summary>
    /// Starts the mod system server-side.
    /// </summary>
    /// <param name="serverApi">API for interacting with game engine server-side.</param>
    protected abstract void Start(ICoreServerAPI serverApi);

    /// <summary>
    /// A method invoked when an exception is thrown during attempting to start the mod system server-side.
    /// </summary>
    /// <param name="serverApi">API for interacting with game engine server-side.</param>
    /// <param name="ex">The exception thrown during attempting to start the mod system.</param>
    protected virtual void OnStartFailed(ICoreServerAPI serverApi, Exception ex) { }

    [GeneratedRegex("(ModSystem|System)$")]
    private static partial Regex GetModSystemNamePartRegex();
}
