using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    public class Setter : IValueStoreSetter, IValue
    {
        public Setter()
        {
        }

        public Setter(AvaloniaProperty property, object? value)
        {
            Property = property;
            Value = value;
        }

        public AvaloniaProperty? Property { get; set; }
        public object? Value { get; set; }
        AvaloniaProperty IValue.Property => EnsureProperty();

        IValue IValueStoreSetter.Instance(StyleInstance instance, IStyleable target)
        {
            _ = Property ?? throw new InvalidOperationException("Setter.Property must be set.");

            if (Value is IBinding binding)
            {
                return new SetterBindingInstance(instance, Property, binding);
            }
            else
            {
                if (!Property.IsValidValue(Value))
                    throw new InvalidCastException($"Setter value '{Value}' is not a valid value for property '{Property}'.");
                return this;
            }
        }

        bool IValue.TryGetValue(out object? value)
        {
            value = Value;
            return true;
        }

        private AvaloniaProperty EnsureProperty()
        {
            return Property ?? throw new InvalidOperationException("Setter.Property must be set.");
        }
    }
}
