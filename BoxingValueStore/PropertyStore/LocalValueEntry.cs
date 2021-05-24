using System;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class LocalValueEntry : IValue, IObserver<object?>, IDisposable
    {
        private readonly ValueStore _owner;
        private IDisposable? _bindingSubscription;
        private Optional<object?> _value;

        public LocalValueEntry(ValueStore owner, AvaloniaProperty property)
        {
            _owner = owner;
            Property = property;
        }

        public AvaloniaProperty Property { get; }

        public IDisposable AddBinding(IObservable<object?> source)
        {
            _bindingSubscription?.Dispose();
            _bindingSubscription = source.Subscribe(this);
            return this;
        }

        public void ClearValue()
        {
            if (_value.HasValue)
            {
                _value = default;
                _owner.LocalValueChanged(Property);
            }
        }

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            ClearValue();
        }

        public void SetValue(object? value)
        {
            if (_value != value)
            {
                _value = value;
                _owner.LocalValueChanged(Property);
            }
        }

        public bool TryGetValue(out object? value)
        {
            if (_value.HasValue)
            {
                value = _value.Value;
                return true;
            }

            value = default;
            return false;
        }

        void IObserver<object?>.OnCompleted() => ClearValue();
        void IObserver<object?>.OnError(Exception error) => ClearValue();

        void IObserver<object?>.OnNext(object? value)
        {
            if (value == BindingOperations.DoNothing)
                return;
            else if (value != AvaloniaProperty.UnsetValue)
                SetValue(value);
            else
                ClearValue();
        }
    }
}
