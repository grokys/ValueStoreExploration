﻿using System;
using System.Collections.Generic;
using Avalonia.Data;
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

        public Style(Func<Selector?, Selector> selector)
        {
            Selector = selector(null);
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

            var setterInstances = new List<ISetterInstance>();
            var canInstanceInPlace = true;

            foreach (var setter in _setters)
            {
                var setterInstance = setter.Instance(target);
                setterInstances.Add(setterInstance);
                canInstanceInPlace &= setterInstance == setter;
            }

            var instance = new StyleInstance(setterInstances, match.Value.Activator);

            if (canInstanceInPlace)
                _sharedInstance = instance;

            return instance;
        }
    }
}