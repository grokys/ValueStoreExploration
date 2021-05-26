using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    internal class SetterBindingInstance : IValue, ISetterInstance, IObserver<object?>
    {
        private static readonly object s_finished = new object();
        private readonly IBinding _binding;
        private ValueStore? _owner;
        private InstancedBinding? _instancedBinding;
        private object? _value = AvaloniaProperty.UnsetValue;

        public SetterBindingInstance(AvaloniaProperty property, IBinding binding)
        {
            Property = property;
            _binding = binding;
        }

        public AvaloniaProperty Property { get; }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetValue(out object? value)
        {
            if (_owner is null)
                throw new InvalidOperationException("Cannot get value from unowned BindingValue.");

            if (_value == s_finished)
            {
                value = default;
                return false;
            }

            if (_instancedBinding is null)
            {
                _instancedBinding = _binding.Initiate(_owner.Owner, Property);
                _instancedBinding.Observable.Subscribe(this);
            }

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
            _value = value;
        }
    }
}
