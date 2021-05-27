using System;

namespace Avalonia.Reactive
{
    internal class AvaloniaStyledPropertyObservable<T> : LightweightObservableBase<T?>, IDescription
    {
        private readonly WeakReference<AvaloniaObject> _target;
        private readonly StyledPropertyBase<T> _property;

        public AvaloniaStyledPropertyObservable(
            AvaloniaObject target,
            StyledPropertyBase<T> property)
        {
            _target = new(target);
            _property = property;
        }

        public string Description => $"{_target.GetType().Name}.{_property.Name}";

        public void OnNext(T? value) => PublishNext(value);

        protected override void Subscribed(IObserver<T?> observer, bool first)
        {
            if (_target.TryGetTarget(out var target))
            {
                var value = target.GetValue(_property);
                observer.OnNext(value);
            }
        }
    }
}
