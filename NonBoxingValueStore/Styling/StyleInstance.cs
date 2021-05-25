using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Styling.Activators;

namespace Avalonia.Styling
{
    internal class StyleInstance : ValueFrameBase, IStyleActivatorSink
    {
        private readonly IStyleActivator? _activator;
        private bool _isActivatorSubscribed;
        private bool _isActive;
        private ValueStore? _valueStore;

        public StyleInstance(IReadOnlyList<ISetterInstance> setters, IStyleActivator? activator)
        {
            _activator = activator;
            _isActive = activator is null;
            Priority = activator is object ? BindingPriority.StyleTrigger : BindingPriority.Style;

            for (var i = 0; i < setters.Count; ++i)
            {
                if (setters[i] is IValue value)
                    Add(value);
            }
        }

        public override bool IsActive
        {
            get
            {
                if (_activator is object && !_isActivatorSubscribed)
                {
                    _isActivatorSubscribed = true;
                    _activator.Subscribe(this);
                }

                return _isActive;
            }
        }

        public override BindingPriority Priority { get; }

        public override void SetOwner(ValueStore? store) => _valueStore = store;

        void IStyleActivatorSink.OnNext(bool value, int tag)
        {
            if (_isActive != value)
            {
                _isActive = value;
                _valueStore?.FrameActivationChanged(this);
            }
        }
    }
}
