using System;
using System.Collections.Generic;
using Avalonia.Data;

#nullable enable

namespace Avalonia.PropertyStore
{
    internal class ValueStore
    {
        private int _applyingStyles;
        private readonly List<IValueFrame> _frames = new List<IValueFrame>();
        private LocalValueFrame? _localValues;
        private Dictionary<int, object?>? _effectiveValues;

        public ValueStore(AvaloniaObject owner) => Owner = owner;

        public AvaloniaObject Owner { get; }
        public IReadOnlyList<IValueFrame> Frames => _frames;

        public void BeginStyling() => ++_applyingStyles;

        public void ApplyStyle(IValueFrame style)
        {
            AddFrame(style);

            if (_applyingStyles == 0)
                ReevaluateEffectiveValues();
        }

        public void ClearLocalValue<T>(StyledPropertyBase<T> property)
        {
            if (_localValues?.ClearValue(property) == true)
                ReevaluateEffectiveValue(property);
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
            if (priority == BindingPriority.LocalValue)
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
            else
            {
                var entry = new BindingEntry<T>(property, source, priority);
                AddFrame(entry);
                ReevaluateEffectiveValue(property);
                return entry;
            }
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

        public object? GetValue(AvaloniaProperty property)
        {
            if (TryGetValue(property, out var value))
                return value;
            return GetDefaultValue(property);
        }

        public bool TryGetValue(AvaloniaProperty property, out object? result)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out result))
            {
                return true;
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

        public void ValueChanged(IValueFrame frame, AvaloniaProperty property)
        {
            ReevaluateEffectiveValue(property);
        }

        public void FrameActivationChanged(IValueFrame frame)
        {
            ReevaluateEffectiveValues();
        }

        public void RemoveBindingEntry<T>(BindingEntry<T> entry)
        {
            _frames.Remove(entry);
            ReevaluateEffectiveValue(entry.Property);
        }

        private void AddFrame(IValueFrame frame)
        {
            var index = _frames.BinarySearch(frame, FrameComparer.Instance);
            if (index < 0)
                index = ~index;
            _frames.Insert(index, frame);
            frame.SetOwner(this);
        }

        private void ReevaluateEffectiveValue(AvaloniaProperty property)
        {
            object? newValue = GetDefaultValue(property);
            var oldValue = TryGetValue(property, out var v) ? v : newValue;

            if (ReevaluateEffectiveValue(property, out var value, out var priority))
            {
                _effectiveValues ??= new Dictionary<int, object?>();
                _effectiveValues[property.Id] = value;
                newValue = value;
            }
            else if (_effectiveValues is object)
            {
                _effectiveValues.Remove(property.Id);
            }

            RaisePropertyChanged(property, oldValue, newValue, priority);
        }

        private bool ReevaluateEffectiveValue(
            AvaloniaProperty property,
            out object? result,
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
                        if (value.TryGetValue(out result))
                        {
                            priority = frame.Priority;
                            return true;
                        }
                    }
                }
            }

            result = default;
            priority = BindingPriority.Unset;
            return false;
        }

        private void ReevaluateEffectiveValues()
        {
            var newValues = new Dictionary<int, object?>();
            var priorities = new Dictionary<int, BindingPriority>();

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
                        newValues.Add(value.Property.Id, v);
                        priorities.Add(value.Property.Id, frame.Priority);
                    }
                }
            }

            var oldValues = _effectiveValues;
            var registry = AvaloniaPropertyRegistry.Instance;
            _effectiveValues = newValues;

            foreach (var (id, newValue) in newValues)
            {
                var property = registry.FindRegistered(id);

                if (property is object)
                {
                    if (oldValues is null || !oldValues.TryGetValue(id, out var oldValue))
                        oldValue = GetDefaultValue(property);
                    RaisePropertyChanged(property, oldValue, newValue, priorities[id]);
                }
                else
                {
                    // TODO: Log error. Non-registered property changed.
                }

                oldValues?.Remove(id);
            }

            if (oldValues is object)
            {
                foreach (var (id, oldValue) in oldValues)
                {
                    var property = registry.FindRegistered(id);

                    if (property is object)
                    {
                        var newValue = GetDefaultValue(property);
                        RaisePropertyChanged(property, oldValue, newValue, BindingPriority.Unset);
                    }
                    else
                    {
                        // TODO: Log error. Non-registered property changed.
                    }
                }
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
            if (!Equals(oldValue, newValue))
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
