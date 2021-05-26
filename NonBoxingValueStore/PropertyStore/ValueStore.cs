using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class ValueStore
    {
        private int _applyingStyles;
        private readonly List<IValueFrame> _frames = new List<IValueFrame>();
        private LocalValueFrame? _localValues;
        private Dictionary<int, IValue>? _effectiveValues;

        public ValueStore(AvaloniaObject owner) => Owner = owner;

        public AvaloniaObject Owner { get; }

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
                _localValues = new LocalValueFrame(this);
                AddFrame(_localValues);
            }

            var result = _localValues.AddBinding(property, source);
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
                _localValues = new LocalValueFrame(this);
                AddFrame(_localValues);
            }

            _localValues.SetValue(property, value);
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            if (TryGetValue(property, out var value))
                return value;
            return property.GetDefaultValue(Owner.GetType());
        }

        public bool TryGetValue(AvaloniaProperty property, out object? result)
        {
            if (_effectiveValues is object &&
                _effectiveValues.TryGetValue(property.Id, out var value))
            {
                return value.TryGetValue(out result);
            }

            result = default;
            return false;
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

        public void ValueChanged(
            IValueFrame frame,
            AvaloniaProperty property,
            object? oldValue)
        {
            ReevaluateEffectiveValue(property, oldValue);
        }

        public void ValueChanged<T>(
            IValueFrame frame,
            StyledPropertyBase<T> property,
            Optional<T> oldValue)
        {
            ReevaluateEffectiveValue(property, oldValue);
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

        private void ReevaluateEffectiveValue(AvaloniaProperty property, object? oldValue)
        {
            var newValue = AvaloniaProperty.UnsetValue;

            if (ReevaluateEffectiveValue(property, out var value, out var priority))
            {
                _effectiveValues ??= new Dictionary<int, IValue>();
                _effectiveValues[property.Id] = value;
                value.TryGetValue(out newValue);
            }
            else if (_effectiveValues is object)
            {
                _effectiveValues.Remove(property.Id);
            }

            RaisePropertyChanged(property, oldValue, newValue, priority);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property)
        {
            var oldValue = TryGetValue(property, out var value) ? new Optional<T>(value) : default;
            ReevaluateEffectiveValue(property, oldValue);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property, Optional<T> oldValue)
        {
            Optional<T> newValue = default;

            if (ReevaluateEffectiveValue(property, out var value, out var priority))
            {
                _effectiveValues ??= new Dictionary<int, IValue>();
                _effectiveValues[property.Id] = value;
                if (((IValue<T>)value).TryGetValue(out var v))
                    newValue = v;
            }
            else if (_effectiveValues is object)
            {
                _effectiveValues.Remove(property.Id);
            }

            RaisePropertyChanged(property, oldValue, newValue, priority);
        }

        private bool ReevaluateEffectiveValue(
            AvaloniaProperty property,
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
            var newValues = DictionaryPool<int, IValue>.Get();
            var priorities = DictionaryPool<int, BindingPriority>.Get();

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
                        priorities.Add(value.Property.Id, frame.Priority);
                    }
                }
            }

            var oldValues = _effectiveValues;
            var registry = AvaloniaPropertyRegistry.Instance;
            _effectiveValues = newValues;

            foreach (var (id, newValueEntry) in newValues)
            {
                var property = registry.FindRegistered(id);

                if (property is object)
                {
                    IValue? oldValueEntry = null;
                    object? oldValue = AvaloniaProperty.UnsetValue;
                    object? newValue = AvaloniaProperty.UnsetValue;

                    oldValues?.TryGetValue(id, out oldValueEntry);
                    oldValueEntry?.TryGetValue(out oldValue);
                    newValueEntry?.TryGetValue(out newValue);

                    RaisePropertyChanged(property, oldValue, newValue, priorities[id]);
                }
                else
                {
                    // TODO: Log error. Non-registered property changed.
                }

                oldValues?.Remove(id);
            }

            DictionaryPool<int, BindingPriority>.Release(priorities);

            if (oldValues is object)
            {
                foreach (var (id, oldValueEntry) in oldValues)
                {
                    var property = registry.FindRegistered(id);

                    if (property is object)
                    {
                        var newValue = AvaloniaProperty.UnsetValue;
                        var oldValue = AvaloniaProperty.UnsetValue;
                        oldValueEntry?.TryGetValue(out oldValue);
                        RaisePropertyChanged(property, oldValue, newValue, BindingPriority.Unset);
                    }
                    else
                    {
                        // TODO: Log error. Non-registered property changed.
                    }
                }

                DictionaryPool<int, IValue>.Release(oldValues);
            }
        }

        private object? GetDefaultValue(AvaloniaProperty property)
        {
            return ((IStyledPropertyAccessor)property).GetDefaultValue(Owner.GetType());
        }

        private void RaisePropertyChanged(
            AvaloniaProperty property,
            object? oldValue,
            object? newValue,
            BindingPriority priority)
        {
            if (oldValue == AvaloniaProperty.UnsetValue)
                oldValue = GetDefaultValue(property);
            if (newValue == AvaloniaProperty.UnsetValue)
                newValue = GetDefaultValue(property);

            if (!Equals(oldValue, newValue))
            {
                property.RaisePropertyChanged(Owner, oldValue, newValue, priority);
            }
        }

        private void RaisePropertyChanged<T>(
            StyledPropertyBase<T> property,
            Optional<T> oldValue,
            Optional<T> newValue,
            BindingPriority priority)
        {
            if (!oldValue.HasValue)
                oldValue = property.GetDefaultValue(Owner.GetType());
            if (!newValue.HasValue)
                newValue = property.GetDefaultValue(Owner.GetType());

            if (oldValue != newValue)
            {
                Owner.RaisePropertyChanged(property, oldValue, newValue, priority);
            }
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
