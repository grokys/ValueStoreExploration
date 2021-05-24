using System;
using Avalonia.Data;

namespace Avalonia.Benchmarks
{
    internal class TestBindingObservable<T> : IObservable<BindingValue<T?>>, IDisposable
    {
        private T? _value;
        private Exception? _error;
        private IObserver<BindingValue<T?>>? _observer;
        private bool _completed;

        public TestBindingObservable(T? value = default) => _value = value;

        public IDisposable Subscribe(IObserver<BindingValue<T?>> observer)
        {
            if (_observer is object)
                throw new InvalidOperationException("The observable can only be subscribed once.");

            if (_error is object)
                observer.OnError(_error);
            else if (_completed)
                observer.OnCompleted();
            else
                _observer = observer;
            observer.OnNext(_value);

            return this;
        }

        public void Dispose() => _observer = null;
        public void OnNext(T? value) => _observer?.OnNext(value);

        public void PublishCompleted()
        {
            _observer?.OnCompleted();
            _completed = true;
            _observer = null;
        }

        protected void PublishError(Exception error)
        {
            _observer?.OnError(error);
            _error = error;
            _observer = null;
        }
    }
}
