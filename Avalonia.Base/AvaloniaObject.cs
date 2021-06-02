using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Reactive;
using Avalonia.Threading;

namespace Avalonia
{
    public class AvaloniaObject : IAvaloniaObject
    {
        private readonly ValueStore _values;
        private Dictionary<AvaloniaProperty, object>? _observables;

        public AvaloniaObject()
        {
            VerifyAccess();
            _values = new ValueStore(this);
        }

        public event EventHandler<AvaloniaPropertyChangedEventArgs>? PropertyChanged;

        public IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            _ = source ?? throw new ArgumentNullException(nameof(source));
            VerifyAccess();

            return _values.AddBinding(property, source, priority);
        }

        public IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority = BindingPriority.LocalValue)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            _ = source ?? throw new ArgumentNullException(nameof(source));
            VerifyAccess();

            return _values.AddBinding(property, source, priority);
        }

        public void ClearValue<T>(StyledPropertyBase<T> property)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            _values.ClearLocalValue(property);
        }

        public void CoerceValue<T>(StyledPropertyBase<T> property)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            throw new NotImplementedException();
        }

        public IObservable<T?> GetObservable<T>(StyledPropertyBase<T> property)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

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
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            return _values.GetValue(property);
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            return _values.IsAnimating(property);
        }

        public bool IsSet(AvaloniaProperty property)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            return _values.IsSet(property);
        }

        public void SetValue<T>(StyledPropertyBase<T> property, T value)
        {
            _ = property ?? throw new ArgumentNullException(nameof(property));
            VerifyAccess();

            _values.SetLocalValue(property, value);
        }

        public void VerifyAccess() => Dispatcher.UIThread.VerifyAccess();

        protected void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private protected void ApplyStyle(IValueFrame frame) => _values.ApplyStyle(frame);
        private protected void BeginStyling() => _values.BeginStyling();
        private protected void EndStyling() => _values.EndStyling();

        internal virtual int GetInheritanceChildCount() => 0;
        internal virtual AvaloniaObject GetInheritanceChild(int index) => throw new IndexOutOfRangeException();
        private protected virtual AvaloniaObject? GetInheritanceParent => null;
        
        private protected void InheritanceParentChanged(AvaloniaObject? parent)
        {
            _values.InheritanceParentChanged(parent?._values);
        }
        
        internal ValueStore GetValueStore() => _values;

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
