using System;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class LocalValueFrame : ValueFrameBase
    {
        public override bool IsActive => true;
        public override BindingPriority Priority => BindingPriority.LocalValue;

        public IDisposable AddBinding<T>(
            ValueStore owner,
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source)
        {
            if (TryGet(property, out var entry))
            {
                return ((LocalValueEntry<T>)entry).AddBinding(source);
            }

            var e = new LocalValueEntry<T>(owner, property);
            Add(e);
            return e.AddBinding(source);
        }

        public bool ClearValue(AvaloniaProperty property) => Remove(property);

        public void SetValue<T>(ValueStore owner, StyledPropertyBase<T> property, T? value)
        {
            if (TryGet(property, out var entry))
            {
                ((LocalValueEntry<T>)entry).SetValue(value);
                return;
            }

            var e = new LocalValueEntry<T>(owner, property);
            Add(e);
            e.SetValue(value);
        }
    }
}
