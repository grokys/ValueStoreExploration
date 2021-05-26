using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class BindingEntry<T> : IValue,
        IValueFrame, 
        IObserver<BindingValue<T>>,
        IList<IValue>,
        IDisposable
    {
        private readonly IObservable<BindingValue<T>> _source;
        private IDisposable? _bindingSubscription;
        private ValueStore? _owner;
        private object? _value = AvaloniaProperty.UnsetValue;

        public BindingEntry(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
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
            _owner?.RemoveBindingEntry(this);
        }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetValue(out object? value)
        {
            StartIfNecessary();
            value = _value;
            return value != AvaloniaProperty.UnsetValue;
        }

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
        void IObserver<BindingValue<T>>.OnCompleted() => Dispose();
        void IObserver<BindingValue<T>>.OnError(Exception error) => Dispose();

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else
                SetValue(AvaloniaProperty.UnsetValue);
        }

        private void SetValue(object? value)
        {
            if (!Equals(_value, value))
            {
                _value = value;
                _owner?.ValueChanged(this, Property);
            }
        }

        private void StartIfNecessary()
        {
            if (_bindingSubscription is null)
            {
                _bindingSubscription = _source.Subscribe(this);
            }
        }
    }
}
