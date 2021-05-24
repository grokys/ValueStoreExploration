using System;
using System.Collections.Generic;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class LocalValueFrame : IValueFrame
    {
        private SortedList<int, IValue> _entries = new();

        public bool IsActive => true;
        public BindingPriority Priority => BindingPriority.LocalValue;
        public IList<IValue> Values => _entries.Values;

        public IDisposable AddBinding<T>(ValueStore owner, StyledPropertyBase<T> property, IObservable<BindingValue<T>> source)
        {
            if (_entries.TryGetValue(property.Id, out var entry))
            {
                return ((LocalValueEntry<T>)entry).AddBinding(source);
            }

            var e = new LocalValueEntry<T>(owner, property);
            _entries.Add(property.Id, e);
            return e.AddBinding(source);
        }

        public bool ClearValue(AvaloniaProperty property)
        {
            return _entries.Remove(property.Id);
        }

        public void SetValue<T>(ValueStore owner, StyledPropertyBase<T> property, T? value)
        {
            if (_entries.TryGetValue(property.Id, out var entry))
            {
                ((LocalValueEntry<T>)entry).SetValue(value);
                return;
            }

            var e = new LocalValueEntry<T>(owner, property);
            _entries.Add(property.Id, e);
            e.SetValue(value);
        }

        public bool TryGetValue<T>(AvaloniaProperty property, out T? value)
        {
            if (_entries.TryGetValue(property.Id, out var v) && ((IValue<T>)v).TryGetValue(out value))
                return true;
            value = default;
            return false;
        }
    }
}
