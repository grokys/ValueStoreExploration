using System;
using System.Collections.Generic;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class LocalValueEntry<T> : IValue<T>, IObserver<BindingValue<T>>, IDisposable
    {
        private readonly ValueStore _owner;
        private IDisposable? _bindingSubscription;
        private bool _hasValue;
        private T? _value;

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
            if (_hasValue)
            {
                _hasValue = false;
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
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _owner.LocalValueChanged(Property);
            }
        }

        public bool TryGetValue(out T? value)
        {
            value = _value;
            return _hasValue;
        }

        public bool TryGetValue(out object? value)
        {
            value = _value;
            return _hasValue;
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
