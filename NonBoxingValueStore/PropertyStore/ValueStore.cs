using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class ValueStore
    {
        private readonly AvaloniaObject _owner;
        private int _applyingStyles;
        private readonly List<IValueFrame> _frames = new List<IValueFrame>();
        private LocalValueFrame? _localValues;
        private Dictionary<int, IValue>? _effectiveValues;

        public ValueStore(AvaloniaObject owner) => _owner = owner;

        public void BeginStyling() => ++_applyingStyles;

        public void ApplyStyle(IValueFrame style)
        {
            AddFrame(style);

            if (_applyingStyles == 0)
                ReevaluateEffectiveValues();
        }

        public void EndStyling()
        {
            if (--_applyingStyles == 0)
                ReevaluateEffectiveValues();
        }

        public IDisposable AddBinding<T>(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority)
        {
            if (_localValues is null)
            {
                _localValues = new LocalValueFrame();
                AddFrame(_localValues);
            }

            var result = _localValues.AddBinding(this, property, source);
            ReevaluateEffectiveValue(property);
            return result;
        }

        public void ClearLocalValue<T>(StyledPropertyBase<T> property)
        {
            if (_localValues?.ClearValue(property) == true)
                ReevaluateEffectiveValue(property);
        }

        public void SetLocalValue<T>(StyledPropertyBase<T> property, T? value)
        {
            if (_localValues is null)
            {
                _localValues = new LocalValueFrame();
                AddFrame(_localValues);
            }

            _localValues.SetValue(this, property, value);
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            if (TryGetValue(property, out var value))
                return value;
            return property.GetDefaultValue(_owner.GetType());
        }

        public bool TryGetValue<T>(StyledPropertyBase<T> property, out T? result)
        {
            if (_effectiveValues is object &&
                _effectiveValues.TryGetValue(property.Id, out var value))
            {
                if (value is IValue<T> typed)
                {
                    return typed.TryGetValue(out result); 
                }
                else if (value.TryGetValue(out var untyped))
                {
                    result = (T?)untyped;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public bool IsSet(AvaloniaProperty property)
        {
            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];
                var values = frame.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];
                    if (value.Property == property && value.TryGetValue(out _))
                        return true;
                }
            }

            return false;
        }

        public void LocalValueChanged<T>(StyledPropertyBase<T> property)
        {
            ReevaluateEffectiveValue(property);
        }

        public void FrameActivationChanged(IValueFrame frame)
        {
            ReevaluateEffectiveValues();
        }

        private void AddFrame(IValueFrame frame)
        {
            var index = _frames.BinarySearch(frame, FrameComparer.Instance);
            if (index < 0)
                index = ~index;
            _frames.Insert(index, frame);
            frame.SetOwner(this);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property)
        {
            T? newValue = property.GetDefaultValue(_owner.GetType());
            var oldValue = TryGetValue(property, out var v) ? v : newValue;

            if (ReevaluateEffectiveValue(property, out var value, out var priority))
            {
                _effectiveValues ??= new Dictionary<int, IValue>();
                _effectiveValues[property.Id] = value;
                ((IValue<T>)value).TryGetValue(out newValue);
            }
            else if (_effectiveValues is object)
            {
                _effectiveValues.Remove(property.Id);
            }

            if (!EqualityComparer<T>.Default.Equals(oldValue!, newValue!))
            {
                _owner.RaisePropertyChanged(property, oldValue, newValue, priority);
            }
        }

        private bool ReevaluateEffectiveValue<T>(
            StyledPropertyBase<T> property,
            [NotNullWhen(true)] out IValue? result,
            out BindingPriority priority)
        {
            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];

                if (!frame.IsActive)
                    continue;

                var values = frame.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];

                    if (value.Property == property)
                    {
                        priority = frame.Priority;
                        result = value;
                        return true;
                    }
                }
            }

            result = default;
            priority = BindingPriority.Unset;
            return false;
        }

        private void ReevaluateEffectiveValues()
        {
            var newValues = new Dictionary<int, IValue>();

            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];

                if (!frame.IsActive)
                    continue;

                var values = frame.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];

                    if (!newValues.ContainsKey(value.Property.Id) &&
                        value.TryGetValue(out var v))
                    {
                        newValues.Add(value.Property.Id, value);
                    }
                }
            }

            var oldValues = newValues;
            var registry = AvaloniaPropertyRegistry.Instance;
            _effectiveValues = newValues;
        }

        private class FrameComparer : IComparer<IValueFrame>
        {
            public static readonly FrameComparer Instance = new FrameComparer();
            public int Compare(IValueFrame? x, IValueFrame? y)
            {
                var result = y!.Priority - x!.Priority;
                return result != 0 ? result : -1;
            }
        }
    }
}
