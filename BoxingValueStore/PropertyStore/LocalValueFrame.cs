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

        public IDisposable AddBinding<T>(ValueStore owner, StyledPropertyBase<T> property, IObservable<object?> source)
        {
            if (_entries.TryGetValue(property.Id, out var entry))
            {
                return ((LocalValueEntry)entry).AddBinding(source);
            }

            var e = new LocalValueEntry(owner, property);
            _entries.Add(property.Id, e);
            return e.AddBinding(source);
        }

        public void SetValue<T>(ValueStore owner, StyledPropertyBase<T> property, T? value)
        {
            if (_entries.TryGetValue(property.Id, out var entry))
            {
                ((LocalValueEntry)entry).SetValue(value);
                return;
            }

            var e = new LocalValueEntry(owner, property);
            _entries.Add(property.Id, e);
            e.SetValue(value);
        }

        public bool TryGetValue<T>(AvaloniaProperty property, out IValue? value)
        {
            return _entries.TryGetValue(property.Id, out value);
        }
    }
}
