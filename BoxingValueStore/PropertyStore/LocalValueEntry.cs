using System;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class LocalValueEntry<T> : IValue, IObserver<BindingValue<T>>, IDisposable
    {
        private readonly LocalValueFrame _owner;
        private IDisposable? _bindingSubscription;
        private object? _value = AvaloniaProperty.UnsetValue;

        public LocalValueEntry(LocalValueFrame owner, AvaloniaProperty property)
        {
            _owner = owner;
            Property = property;
        }

        public AvaloniaProperty Property { get; }

        public IDisposable AddBinding(IObservable<BindingValue<T>> source)
        {
            _bindingSubscription?.Dispose();
            _bindingSubscription = source.Subscribe(this);
            return this;
        }

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            SetValue(AvaloniaProperty.UnsetValue);
        }

        public void SetValue(object? value)
        {
            if (_value != value)
            {
                _value = value;
                _owner.ValueStore.ValueChanged(_owner, Property);
            }
        }

        public bool TryGetValue(out object? value)
        {
            value = _value;
            return value != AvaloniaProperty.UnsetValue;
        }

        void IObserver<BindingValue<T>>.OnCompleted() => SetValue(AvaloniaProperty.UnsetValue);
        void IObserver<BindingValue<T>>.OnError(Exception error) => SetValue(AvaloniaProperty.UnsetValue);

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else if (value.Type == BindingValueType.BindingError)
                SetValue(AvaloniaProperty.UnsetValue);
        }
    }
}
