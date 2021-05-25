using System;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    class Setter<T> : ISetter, ISetterInstance, IValue<T>
    {
        public Setter(AvaloniaProperty<T> property, T value)
        {
            Property = property;
            Value = value;
        }

        public AvaloniaProperty<T>? Property { get; }
        public T? Value { get; }
        AvaloniaProperty IValue.Property => EnsureProperty();

        public ISetterInstance Instance(IStyleable target) => this;

        bool IValue<T>.TryGetValue(out T? value)
        {
            value = Value;
            return true;
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
