using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia.PropertyStore
{
    internal class BindingEntry<T> : IValue<T>,
        IValueFrame, 
        IObserver<BindingValue<T>>,
        IObserver<T?>,
        IList<IValue>,
        IDisposable
    {
        private readonly object _source;
        private IDisposable? _bindingSubscription;
        private ValueStore? _owner;
        private bool _hasValue;
        private T? _value;

        public BindingEntry(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
        }

        public BindingEntry(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
        }

        public bool HasValue
        {
            get
            {
                StartIfNecessary();
                return _hasValue;
            }
        }
        
        public bool IsActive => true;
        public BindingPriority Priority { get; }
        public StyledPropertyBase<T> Property { get; }
        AvaloniaProperty IValue.Property => Property;
        public IList<IValue> Values => this;
        int ICollection<IValue>.Count => 1;
        bool ICollection<IValue>.IsReadOnly => true;
        
        IValue IList<IValue>.this[int index] 
        { 
            get => this;
            set => throw new NotImplementedException(); 
        }

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            BindingCompleted();
        }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetValue(out T? value)
        {
            StartIfNecessary();
            value = _value;
            return _hasValue;
        }

        public bool TryGetValue(out object? value)
        {
            StartIfNecessary();
            value = _value;
            return _hasValue;
        }

        public void OnCompleted() => BindingCompleted();
        public void OnError(Exception error) => BindingCompleted();
        void IObserver<T?>.OnNext(T? value) => SetValue(value);

        int IList<IValue>.IndexOf(IValue item) => throw new NotImplementedException();
        void IList<IValue>.Insert(int index, IValue item) => throw new NotImplementedException();
        void IList<IValue>.RemoveAt(int index) => throw new NotImplementedException();
        void ICollection<IValue>.Add(IValue item) => throw new NotImplementedException();
        void ICollection<IValue>.Clear() => throw new NotImplementedException();
        bool ICollection<IValue>.Contains(IValue item) => throw new NotImplementedException();
        void ICollection<IValue>.CopyTo(IValue[] array, int arrayIndex) => throw new NotImplementedException();
        bool ICollection<IValue>.Remove(IValue item) => throw new NotImplementedException();
        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else
                ClearValue();
        }

        private void ClearValue()
        {
            _ = _owner ?? throw new AvaloniaInternalException("BindingEntry has no owner.");

            var oldValue = _hasValue ? new Optional<T>(_value) : default;

            if (_bindingSubscription is null)
                _owner.RemoveBindingEntry(this, oldValue);
            else if (_hasValue)
            {
                _hasValue = false;
                _value = default;
                _owner.ValueChanged(this, Property, oldValue);
            }
        }

        private void SetValue(T? value)
        {
            _ = _owner ?? throw new AvaloniaInternalException("BindingEntry has no owner.");

            if (Property.ValidateValue?.Invoke(value) == false)
            {
                value = Property.GetDefaultValue(_owner.Owner.GetType());
            }

            if (!_hasValue || !EqualityComparer<T>.Default.Equals(_value, value))
            {
                var oldValue = _hasValue ? new Optional<T>(_value) : default;
                _value = value;
                _hasValue = true;
                _owner.ValueChanged(this, Property, oldValue);
            }
        }

        private void StartIfNecessary()
        {
            if (_bindingSubscription is null)
            {
                // Prevent reentrancy by first assigning the subscription to a dummy
                // non-null value.
                _bindingSubscription = Disposable.Empty;

                if (_source is IObservable<BindingValue<T>> b)
                    _bindingSubscription = b.Subscribe(this);
                else
                    _bindingSubscription = ((IObservable<T?>)_source).Subscribe(this);
            }
        }

        private void BindingCompleted()
        {
            _bindingSubscription = null;
            ClearValue();
        }
    }
}
