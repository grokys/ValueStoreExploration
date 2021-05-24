using System;
using Avalonia.Data;

namespace Avalonia
{
    public class AvaloniaObject : IAvaloniaObject
    {
        public void AddInheritanceChild(IAvaloniaObject child)
        {
            throw new NotImplementedException();
        }

        public IDisposable Bind<T>(StyledPropertyBase<T> property, IObservable<BindingValue<T>> source, BindingPriority priority = BindingPriority.LocalValue)
        {
            throw new NotImplementedException();
        }

        public void ClearValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public void CoerceValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            throw new NotImplementedException();
        }

        public bool IsSet(AvaloniaProperty property)
        {
            throw new NotImplementedException();
        }

        public void RemoveInheritanceChild(IAvaloniaObject child)
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(StyledPropertyBase<T> property, T value)
        {
            throw new NotImplementedException();
        }

        internal void RaisePropertyChanged()
        {

        }
    }
}
