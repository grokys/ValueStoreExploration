﻿using System;
using Avalonia.Data;
using Avalonia.PropertyStore;

namespace Avalonia
{
    public class AvaloniaObject : IAvaloniaObject
    {
        private readonly ValueStore _values;

        public AvaloniaObject()
        {
            _values = new ValueStore(this);
        }

        public event EventHandler<AvaloniaPropertyChangedEventArgs>? PropertyChanged;

        public void AddInheritanceChild(IAvaloniaObject child)
        {
            throw new NotImplementedException();
        }

        public IDisposable Bind<T>(StyledPropertyBase<T> property, IObservable<BindingValue<T>> source, BindingPriority priority = BindingPriority.LocalValue)
        {
            return _values.AddBinding(property, source, priority);
        }

        public void ClearValue<T>(StyledPropertyBase<T> property) => _values.ClearLocalValue(property);

        public void CoerceValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            return (T?)_values.GetValue(property);
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            throw new NotImplementedException();
        }

        public bool IsSet(AvaloniaProperty property) => _values.IsSet(property);

        public void RemoveInheritanceChild(IAvaloniaObject child)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(StyledPropertyBase<T> property, T value)
        {
            _values.SetLocalValue(property, value);
        }

        protected void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private protected void ApplyStyle(IValueFrame frame) => _values.ApplyStyle(frame);
        private protected void BeginStyling() => _values.BeginStyling();
        private protected void EndStyling() => _values.EndStyling();

        internal void RaisePropertyChanged(
            AvaloniaProperty property,
            object? oldValue,
            object? newValue,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            OnPropertyChanged(new AvaloniaPropertyChangedEventArgs(
                this,
                property,
                oldValue,
                newValue,
                priority));
        }
    }
}
