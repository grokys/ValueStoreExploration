using System;
using System.Collections.Generic;
using Avalonia.PropertyStore;

namespace Avalonia.Styling
{
    public class Style : IStyle
    {
        private readonly List<ISetter> _setters = new List<ISetter>();
        private StyleInstance? _sharedInstance;

        public Style()
        {
        }

        public Style(Func<Selector, Selector> selector)
        {
            Selector = selector(null!);
        }

        public Selector? Selector { get; }
        public IList<ISetter> Setters => _setters;

        internal IValueFrame? Instance(IStyleable target)
        {
            var match = Selector?.Evaluate(target, true);

            if (match?.IsMatch != true)
                return null;

            if (_sharedInstance is object)
                return _sharedInstance;

            var instance = new StyleInstance(match.Value.Activator);
            var canInstanceInPlace = true;

            foreach (var setter in _setters)
            {
                if (setter is IValueStoreSetter v)
                {
                    var setterInstance = v.Instance(instance, target);
                    instance.Add(setterInstance);
                    canInstanceInPlace &= setterInstance == setter;
                }
            }

            if (canInstanceInPlace)
                _sharedInstance = instance;

            return instance;
        }
    }
}
