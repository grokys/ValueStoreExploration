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

        public StyleInstance(IStyleActivator? activator)
        {
            _activator = activator;
            _isActive = activator is null;
            Priority = activator is object ? BindingPriority.StyleTrigger : BindingPriority.Style;
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

        public ValueStore? ValueStore { get; private set; }

        public override BindingPriority Priority { get; }

        public new void Add(IValueEntry value) => base.Add(value);
        public override void SetOwner(ValueStore? owner) => ValueStore = owner;

        void IStyleActivatorSink.OnNext(bool value, int tag)
        {
            if (_isActive != value)
            {
                _isActive = value;
                ValueStore?.FrameActivationChanged(this);
            }
        }
    }
}
