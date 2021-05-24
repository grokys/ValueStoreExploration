using System;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class LocalValueEntry<T> : IValue<T>, IObserver<BindingValue<T>>, IDisposable
    {
        private readonly ValueStore _owner;
        private IDisposable? _bindingSubscription;
        private Optional<T?> _value;

        public LocalValueEntry(ValueStore owner, StyledPropertyBase<T> property)
        {
            _owner = owner;
            Property = property;
        }

        public StyledPropertyBase<T> Property { get; }
        AvaloniaProperty IValue.Property => Property;

        public IDisposable AddBinding(IObservable<BindingValue<T>> source)
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

        public void SetValue(T? value)
        {
            if (_value != value)
            {
                _value = value;
                _owner.LocalValueChanged(Property);
            }
        }

        public bool TryGetValue(out T? value)
        {
            if (_value.HasValue)
            {
                value = _value.Value;
                return true;
            }

            value = default;
            return false;
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

        void IObserver<BindingValue<T>>.OnCompleted() => ClearValue();
        void IObserver<BindingValue<T>>.OnError(Exception error) => ClearValue();

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else if (value.Type == BindingValueType.BindingError)
                ClearValue();
        }
    }
}
