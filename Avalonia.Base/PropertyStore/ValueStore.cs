﻿using System;
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
        private Dictionary<int, EffectiveValue>? _effectiveValues;

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

                // LocalValue bindings are subscribed immediately in LocalValueEntry so no need to
                // re-evaluate the effective value here.
                return _localValues.AddBinding(property, source);
            }
            else
            {
                var entry = new BindingEntry<T>(property, source, priority);
                AddFrame(entry);
                ReevaluateEffectiveValue(property);
                return entry;
            }
        }

        public IDisposable AddBinding<T>(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority)
        {
            if (priority == BindingPriority.LocalValue)
            {
                if (_localValues is null)
                {
                    _localValues = new LocalValueFrame(this);
                    AddFrame(_localValues);
                }

                // LocalValue bindings are subscribed immediately in LocalValueEntry so no need to
                // re-evaluate the effective value here.
                return _localValues.AddBinding(property, source);
            }
            else
            {
                var entry = new BindingEntry<T>(property, source, priority);
                AddFrame(entry);
                ReevaluateEffectiveValue(property);
                return entry;
            }
        }

        public void ClearLocalValue<T>(StyledPropertyBase<T> property)
        {
            _localValues?.ClearValue(property);
        }

        public void SetLocalValue<T>(StyledPropertyBase<T> property, T? value)
        {
            if (property.ValidateValue?.Invoke(value) == false)
            {
                throw new ArgumentException($"{value} is not a valid value for '{property.Name}.");
            }

            if (_localValues is null)
            {
                _localValues = new LocalValueFrame(this);
                AddFrame(_localValues);
            }

            _localValues.SetValue(property, value);
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var value))
                return value.GetValue<T>();
            return property.GetDefaultValue(Owner.GetType());
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            if (EvaluateEffectiveValue(property, out _, out var priority))
                return priority < BindingPriority.LocalValue;
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
                    if (value.Property == property && value.HasValue)
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
            in Optional<T> oldValue)
        {
            ReevaluateEffectiveValue(property, oldValue);
        }

        public void FrameActivationChanged(IValueFrame frame)
        {
            ReevaluateEffectiveValues();
        }

        public void RemoveBindingEntry<T>(BindingEntry<T> entry, in Optional<T> oldValue)
        {
            _frames.Remove(entry);
            ReevaluateEffectiveValue(entry.Property, oldValue);
        }

        private void AddFrame(IValueFrame frame)
        {
            var index = _frames.BinarySearch(frame, FrameInsertionComparer.Instance);
            if (index < 0)
                index = ~index;
            _frames.Insert(index, frame);
            frame.SetOwner(this);
        }

        private void ReevaluateEffectiveValue(AvaloniaProperty property, object? oldValue)
        {
            var newValue = AvaloniaProperty.UnsetValue;

            if (EvaluateEffectiveValue(property, out var value, out var priority))
                newValue = SetEffectiveValue(property, value);
            else if (_effectiveValues is object)
                _effectiveValues.Remove(property.Id);

            RaisePropertyChanged(property, oldValue, newValue, priority);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var value))
                ReevaluateEffectiveValue(property, value.GetValue<T>());
            else
                ReevaluateEffectiveValue(property, default);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property, in Optional<T> oldValue)
        {
            Optional<T> newValue = default;

            if (EvaluateEffectiveValue(property, out var value, out var priority))
                newValue = SetEffectiveValue(property, value);
            else if (_effectiveValues is object)
                _effectiveValues.Remove(property.Id);

            RaisePropertyChanged(property, oldValue, newValue, priority);
        }

        private bool EvaluateEffectiveValue(
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

                    if (value.Property == property && value.HasValue)
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
            var newValues = DictionaryPool<int, EffectiveValue>.Get();
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

                    if (!newValues.ContainsKey(value.Property.Id) && value.HasValue)
                    {
                        newValues.Add(value.Property.Id, new(value));
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
                    object? oldValue = AvaloniaProperty.UnsetValue;
                    object? newValue = newValueEntry.GetValue();

                    if (oldValues is object && oldValues.TryGetValue(id, out var oldValueEntry))
                        oldValue = oldValueEntry.GetValue();

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
                        var oldValue = oldValueEntry.GetValue();
                        RaisePropertyChanged(property, oldValue, newValue, BindingPriority.Unset);
                    }
                    else
                    {
                        // TODO: Log error. Non-registered property changed.
                    }
                }

                DictionaryPool<int, EffectiveValue>.Release(oldValues);
            }
        }

        private object? SetEffectiveValue(AvaloniaProperty property, IValue value)
        {
            _effectiveValues ??= new();
            _effectiveValues[property.Id] = new(value);
            value.TryGetValue(out var result);
            return result;
        }

        private T? SetEffectiveValue<T>(StyledPropertyBase<T> property, IValue value)
        {
            _effectiveValues ??= new();
            _effectiveValues[property.Id] = new(value);
            ((IValue<T>)value).TryGetValue(out var result);
            return result;
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

        private readonly struct EffectiveValue
        {
            public EffectiveValue(IValue value)
            {
                Entry = value;
            }

            public readonly IValue Entry;

            public object? GetValue()
            {
                Entry.TryGetValue(out var result);
                return result;
            }

            public T? GetValue<T>()
            {
                if (Entry is IValue<T> typed)
                {
                    typed.TryGetValue(out var result);
                    return result;
                }
                else
                {
                    Entry.TryGetValue(out var result);
                    return (T?)result;
                }
            }
        }

        private class FrameInsertionComparer : IComparer<IValueFrame>
        {
            public static readonly FrameInsertionComparer Instance = new FrameInsertionComparer();
            public int Compare(IValueFrame? x, IValueFrame? y)
            {
                var result = y!.Priority - x!.Priority;
                return result != 0 ? result : -1;
            }
        }
    }
}
