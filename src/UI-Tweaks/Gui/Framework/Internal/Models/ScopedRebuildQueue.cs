namespace BitzArt.UI.Tweaks.Gui;

internal sealed class ScopedRebuildQueue
{
    private Dictionary<GuiRenderFragment, GuiTreeBuilder> _pending = [];
    private Dictionary<GuiRenderFragment, GuiTreeBuilder> _active = [];

    internal void Schedule(GuiRenderFragment fragment, GuiTreeBuilder builder) => _pending[fragment] = builder;

    internal void Cancel(GuiRenderFragment fragment)
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
