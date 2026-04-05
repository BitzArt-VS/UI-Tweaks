using System;

namespace BitzArt.UI.Tweaks.Services;

public partial class GameStatusService
{
    private class DetailRecord<T>(GameStatusDetailType detail, string name, Func<T, T>? onUpdate = null) : DetailRecord(detail, name)
    {
        private readonly Func<T, T>? _onUpdate = onUpdate;
        private T? _currentValue;

        public override object? Value => _currentValue;

        public sealed override bool Update(object value)
        {
            if (value is null)
            {
                return false;
            }

            if (value is not T typedValue)
            {
                throw new InvalidOperationException($"Expected value of type {typeof(T)}, but got {value.GetType()}.");
            }

            return Update(typedValue);
        }

        public bool Update(T value)
        {
            value = _onUpdate is not null ? _onUpdate.Invoke(value) : value;

            if (Equals(value, _currentValue))
            {
                return false;
            }

            _currentValue = value;
            return true;
        }
    }

    private abstract class DetailRecord(GameStatusDetailType detail, string name)
    {
        public GameStatusDetailType Detail { get; private init; } = detail;

        public string Name { get; private init; } = name;

        public abstract object? Value { get; }

        public abstract bool Update(object value);
    }
}
