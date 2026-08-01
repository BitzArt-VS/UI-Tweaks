namespace BitzArt.VS.GUI;

/// <summary>
/// Represents a callback through which a GUI component invokes a user-defined handler
/// with an argument. The handler can complete synchronously or asynchronously.
/// </summary>
/// <typeparam name="T">Type of argument provided to the handler.</typeparam>
/// <param name="handler">Work to perform when the callback is invoked.</param>
public readonly struct GuiCallback<T>(Func<T, ValueTask> handler)
{
    private readonly Func<T, ValueTask>? _handler = handler;

    /// <summary>
    /// Invokes the handler with the specified argument.
    /// </summary>
    /// <param name="arg">Argument to provide to the handler.</param>
    /// <returns>A task representing completion of the handler.</returns>
    public ValueTask InvokeAsync(T arg) =>
        _handler?.Invoke(arg) ?? ValueTask.CompletedTask;

    /// <summary>
    /// Converts a synchronous handler to an awaitable callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback<T>(Action<T> handler)
        => new(arg =>
        {
            handler.Invoke(arg);
            return ValueTask.CompletedTask;
        });

    /// <summary>
    /// Converts a task-returning handler to a callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes and awaits <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback<T>(Func<T, Task> handler) =>
        new(arg => new ValueTask(handler.Invoke(arg)));

    /// <summary>
    /// Converts a value-task-returning handler to a callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes and awaits <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback<T>(Func<T, ValueTask> handler) =>
        new(handler);

    /// <summary>
    /// Combines two callbacks into one that invokes their handlers sequentially with the same argument.
    /// </summary>
    /// <param name="first">Callback invoked first.</param>
    /// <param name="second">
    /// Callback invoked after <paramref name="first"/> completes successfully.
    /// </param>
    /// <returns>A callback that represents the combined invocation.</returns>
    public static GuiCallback<T> operator +(GuiCallback<T> first, GuiCallback<T> second)
    {
        if (first._handler is null)
        {
            return second;
        }

        if (second._handler is null)
        {
            return first;
        }

        return new(async arg =>
        {
            await first.InvokeAsync(arg);
            await second.InvokeAsync(arg);
        });
    }
}

/// <summary>
/// Represents user-defined work that a GUI component invokes through a callback.
/// The work can complete synchronously or asynchronously.
/// </summary>
/// <param name="handler">Work to perform when the callback is invoked.</param>
public readonly struct GuiCallback(Func<ValueTask> handler)
{
    private readonly Func<ValueTask>? _handler = handler;

    /// <summary>
    /// Invokes the handler.
    /// </summary>
    /// <returns>A task representing completion of the handler.</returns>
    public ValueTask InvokeAsync() =>
        _handler?.Invoke() ?? ValueTask.CompletedTask;

    /// <summary>
    /// Converts a synchronous handler to an awaitable callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback(Action handler)
        => new(() =>
        {
            handler.Invoke();
            return ValueTask.CompletedTask;
        });

    /// <summary>
    /// Converts a task-returning handler to a callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes and awaits <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback(Func<Task> handler) =>
        new(() => new ValueTask(handler()));

    /// <summary>
    /// Converts a value-task-returning handler to a callback.
    /// </summary>
    /// <param name="handler">Handler to invoke through the callback.</param>
    /// <returns>A callback that invokes and awaits <paramref name="handler"/>.</returns>
    public static implicit operator GuiCallback(Func<ValueTask> handler) =>
        new(handler);

    /// <summary>
    /// Combines two callbacks into one that invokes their handlers sequentially.
    /// </summary>
    /// <param name="first">Callback invoked first.</param>
    /// <param name="second">
    /// Callback invoked after <paramref name="first"/> completes successfully.
    /// </param>
    /// <returns>A callback that represents the combined invocation.</returns>
    public static GuiCallback operator +(GuiCallback first, GuiCallback second)
    {
        if (first._handler is null)
        {
            return second;
        }

        if (second._handler is null)
        {
            return first;
        }

        return new(async () =>
        {
            await first.InvokeAsync();
            await second.InvokeAsync();
        });
    }
}
