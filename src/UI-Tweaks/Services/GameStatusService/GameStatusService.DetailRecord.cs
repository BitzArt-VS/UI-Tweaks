using System;

namespace BitzArt.UI.Tweaks.Services;

public partial class GameStatusService
{
    private class DetailRecord<T>(GameStatusDetailType detail, string name, Func<T, T>? onUpdate = null) : DetailRecord(detail, name)
    {
        private T? _currentValue;
        private readonly Func<T, T>? _onUpdate = onUpdate;

        public override object? Value => _currentValue;

        public bool Update(T value)
        {
            if (Equals(value, _currentValue))
            {
                return false;
            }

            _currentValue = value;
            return true;
        }

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

            typedValue = _onUpdate is not null ? _onUpdate.Invoke(typedValue) : typedValue;

            return Update(typedValue);
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
