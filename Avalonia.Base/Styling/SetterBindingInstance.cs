using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    internal class SetterBindingInstance : IValue, IObserver<object?>
    {
        private static readonly object s_finished = new object();
        private readonly StyleInstance _owner;
        private readonly IBinding _binding;
        private InstancedBinding? _instancedBinding;
        private object? _value = AvaloniaProperty.UnsetValue;

        public SetterBindingInstance(StyleInstance owner, AvaloniaProperty property, IBinding binding)
        {
            _owner = owner;
            _binding = binding;
            Property = property;
        }

        public bool HasValue
        {
            get
            {
                StartIfNecessary();
                return _value != AvaloniaProperty.UnsetValue;
            }
        }

        public AvaloniaProperty Property { get; }

        public bool TryGetValue(out object? value)
        {
            if (_owner.ValueStore is null)
                throw new InvalidOperationException("Cannot get value from unowned BindingValue.");

            if (_value == s_finished)
            {
                value = default;
                return false;
            }

            StartIfNecessary();
            value = _value;
            return value != AvaloniaProperty.UnsetValue;
        }

        void IObserver<object?>.OnCompleted() => _value = s_finished;

        void IObserver<object?>.OnError(Exception error)
        {
            //TODO: Log error
            _value = s_finished;
        }

        void IObserver<object?>.OnNext(object? value)
        {
#if !BOXING
            var oldValue = _value;
#endif
            _value = value;
#if BOXING
            _owner.ValueStore?.ValueChanged(_owner, Property);
#else
            _owner.ValueStore?.ValueChanged(_owner, Property, oldValue);
#endif
        }

        private void StartIfNecessary()
        {
            if (_instancedBinding is null)
            {
                _instancedBinding = _binding.Initiate(_owner.ValueStore.Owner, Property);
                _instancedBinding.Observable.Subscribe(this);
            }
        }
    }
}
