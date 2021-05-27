﻿using System;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class LocalValueFrame : ValueFrameBase
    {
        public LocalValueFrame(ValueStore owner)
        {
            ValueStore = owner;
        }

        public override bool IsActive => true;
        public override BindingPriority Priority => BindingPriority.LocalValue;
        public ValueStore ValueStore { get; }

        public IDisposable AddBinding<T>(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source)
        {
            if (TryGet(property, out var entry))
            {
                return ((LocalValueEntry<T>)entry).AddBinding(source);
            }

            var e = new LocalValueEntry<T>(this, property);
            Add(e);
            return e.AddBinding(source);
        }

        public void ClearValue<T>(StyledPropertyBase<T> property)
        {
            if (TryGet(property, out var entry))
                ((LocalValueEntry<T>)entry).ClearValue();
        }

        public void SetValue<T>(StyledPropertyBase<T> property, T? value)
        {
            if (TryGet(property, out var entry))
            {
                ((LocalValueEntry<T>)entry).SetValue(value);
                return;
            }

            var e = new LocalValueEntry<T>(this, property);
            Add(e);
            e.SetValue(value);
        }

        public void Remove(IValue value) => base.Remove(value.Property);
    }
}
