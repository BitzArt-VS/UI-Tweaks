using Vintagestory.API.Common;

namespace BitzArt.VS.GUI;

internal sealed class GuiCallbackDispatcher(ILogger logger)
{
    public void Dispatch<T>(GuiCallback<T>? callback, T argument)
    {
        if (callback is null)
        {
            return;
        }

        _ = DispatchAsync(callback.Value, argument);
    }

    private async Task DispatchAsync<T>(GuiCallback<T> callback, T argument)
    {
        try
        {
            await callback.InvokeAsync(argument);
        }
        catch (Exception exception)
        {
            logger.Error("An unhandled GUI callback exception occurred.");
            logger.Error(exception);
        }
    }
}
