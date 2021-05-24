using System;
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
            throw new NotImplementedException();
        }

        public void ClearValue<T>(StyledPropertyBase<T> property) => _values.ClearLocalValue(property);

        public void CoerceValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            return _values.GetValue(property);
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

        protected void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        internal void RaisePropertyChanged<T>(
            AvaloniaProperty<T> property,
            Optional<T> oldValue,
            BindingValue<T> newValue,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            OnPropertyChanged(new AvaloniaPropertyChangedEventArgs<T>(
                this,
                property,
                oldValue,
                newValue,
                priority));
        }
    }
}
