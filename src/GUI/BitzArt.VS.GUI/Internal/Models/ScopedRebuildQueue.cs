namespace BitzArt.VS.GUI;

internal sealed class ScopedRebuildQueue
{
    private Dictionary<GuiTreeFragment, GuiTreeBuilder> _pending = [];
    private Dictionary<GuiTreeFragment, GuiTreeBuilder> _active = [];

    internal void Schedule(GuiTreeFragment fragment, GuiTreeBuilder builder) => _pending[fragment] = builder;

    internal void Cancel(GuiTreeFragment fragment)
    {
        _pending.Remove(fragment);
        _active.Remove(fragment);
    }

    internal bool Drain()
    {
        if (_pending.Count == 0)
        {
            return false;
        }

        (_pending, _active) = (_active, _pending);
        while (_active.Count > 0)
        {
            var enumerator = _active.GetEnumerator();
            enumerator.MoveNext();
            var (fragment, builder) = enumerator.Current;
            _active.Remove(fragment);
            builder.Run(fragment);
        }
        return true;
    }
}
