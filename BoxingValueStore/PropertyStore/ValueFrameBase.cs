using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal abstract class ValueFrameBase : IValueFrame
    {
        private readonly SortedList<int, IValue> _values = new();

        public abstract bool IsActive { get; }
        public abstract BindingPriority Priority { get; }
        public IList<IValue> Values => _values.Values;

        public virtual void SetOwner(ValueStore? store)
        {
        }

        public bool TryGet(AvaloniaProperty property, [NotNullWhen(true)] out IValue? value)
        {
            return _values.TryGetValue(property.Id, out value);
        }

        public bool TryGetValue<T>(AvaloniaProperty property, out object? value)
        {
            if (_values.TryGetValue(property.Id, out var v) && v.TryGetValue(out value))
                return true;
            value = default;
            return false;
        }

        protected void Add(IValue value) => _values.Add(value.Property.Id, value);
        protected bool Remove(AvaloniaProperty property) => _values.Remove(property.Id);
    }
}
