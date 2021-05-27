using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Reactive;

namespace Avalonia
{
    public class AvaloniaObject : IAvaloniaObject
    {
        private readonly ValueStore _values;
        private Dictionary<AvaloniaProperty, object>? _observables;

        public AvaloniaObject()
        {
            _values = new ValueStore(this);
        }

        public event EventHandler<AvaloniaPropertyChangedEventArgs>? PropertyChanged;

        public void AddInheritanceChild(IAvaloniaObject child)
        {
            throw new NotImplementedException();
        }

        public IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            return _values.AddBinding(property, source, priority);
        }

        public IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            return _values.AddBinding(property, source, priority);
        }

        public void ClearValue<T>(StyledPropertyBase<T> property) => _values.ClearLocalValue(property);

        public void CoerceValue<T>(StyledPropertyBase<T> property)
        {
            throw new NotImplementedException();
        }

        public IObservable<T?> GetObservable<T>(StyledPropertyBase<T> property)
        {
            _observables ??= new();

            if (_observables.TryGetValue(property, out var o))
                return (IObservable<T>)o;
            else
            {
                var result = new AvaloniaStyledPropertyObservable<T>(this, property);
                _observables.Add(property, result);
                return result;
            }
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            return _values.GetValue(property);
        }

        public bool IsAnimating(AvaloniaProperty property) => _values.IsAnimating(property);
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

        private protected void ApplyStyle(IValueFrame frame) => _values.ApplyStyle(frame);
        private protected void BeginStyling() => _values.BeginStyling();
        private protected void EndStyling() => _values.EndStyling();

        internal void RaisePropertyChanged<T>(
            AvaloniaProperty<T> property,
            Optional<T> oldValue,
            BindingValue<T> newValue,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            var e = AvaloniaPropertyChangedEventArgsPool<T>.Get(
                this,
                property,
                oldValue,
                newValue,
                priority);
            OnPropertyChanged(e);

            if (_observables is object && _observables.TryGetValue(property, out var o))
            {
                var value = e.NewValue.Value;

                // Release the event args here so they can be recycled if raising the change on the
                // observable causes a cascading change.
                AvaloniaPropertyChangedEventArgsPool<T>.Release(e);

                ((AvaloniaStyledPropertyObservable<T?>)o).OnNext(value);
            }
            else
            {
                AvaloniaPropertyChangedEventArgsPool<T>.Release(e);
            }
        }

        internal ValueStore UnitTestGetValueStore() => _values;
    }
}
