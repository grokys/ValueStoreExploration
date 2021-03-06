using System;
using System.Collections.Generic;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class LocalValueEntry<T> : IValueEntry<T>,
        IObserver<BindingValue<T>>,
        IObserver<T?>,
        IDisposable
    {
        private readonly LocalValueFrame _owner;
        private IDisposable? _bindingSubscription;
        private bool _hasValue;
        private T? _value;

        public LocalValueEntry(LocalValueFrame owner, StyledPropertyBase<T> property)
        {
            _owner = owner;
            Property = property;
        }

        public bool HasValue => _hasValue;
        public StyledPropertyBase<T> Property { get; }
        AvaloniaProperty IValueEntry.Property => Property;

        public IDisposable AddBinding(IObservable<BindingValue<T>> source)
        {
            _bindingSubscription?.Dispose();
            _bindingSubscription = source.Subscribe(this);
            return this;
        }

        public IDisposable AddBinding(IObservable<T?> source)
        {
            _bindingSubscription?.Dispose();
            _bindingSubscription = source.Subscribe(this);
            return this;
        }

        public void ClearValue()
        {
            if (_bindingSubscription is null)
                _owner.Remove(this);

            if (_hasValue)
            {
                var oldValue = _hasValue ? new Optional<T>(_value) : default;
                _hasValue = false;
                _value = default;
                _owner.ValueStore.ValueChanged(_owner, this, oldValue);
            }
        }

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            BindingCompleted();
        }

        public void SetValue(T? value)
        {
            if (Property.ValidateValue?.Invoke(value) == false)
            {
                value = Property.GetDefaultValue(_owner.ValueStore.Owner.GetType());
            }

            if (!_hasValue || !EqualityComparer<T>.Default.Equals(_value, value))
            {
                var oldValue = _hasValue ? new Optional<T>(_value) : default;
                _value = value;
                _hasValue = true;
                _owner.ValueStore.ValueChanged<T>(_owner, this, oldValue);
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

        public void OnCompleted() => BindingCompleted();
        public void OnError(Exception error) => BindingCompleted();
        void IObserver<T?>.OnNext(T? value) => SetValue(value);

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else if (value.Type == BindingValueType.BindingError)
                ClearValue();
        }


        private void BindingCompleted()
        {
            _bindingSubscription = null;
            ClearValue();
        }
    }
}
