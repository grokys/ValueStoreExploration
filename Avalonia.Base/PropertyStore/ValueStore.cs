using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class ValueStore
    {
        private int _applyingStyles;
        private readonly List<IValueFrame> _frames = new();
        private InheritanceFrame? _inheritanceFrame;
        private LocalValueFrame? _localValues;
        private Dictionary<int, EffectiveValue>? _effectiveValues;
        private Dictionary<int, EffectiveValue>? _nonAnimatedValues;

        public ValueStore(AvaloniaObject owner) => Owner = owner;

        public AvaloniaObject Owner { get; }
        public IReadOnlyList<IValueFrame> Frames => _frames;
        public InheritanceFrame? InheritanceFrame => _inheritanceFrame;

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
                var effectiveValue = GetEffectiveValue(property);
                var entry = new BindingEntry<T>(property, source, priority);
                AddFrame(entry);

                if (priority < effectiveValue.Priority)
                {
                    var oldValue = effectiveValue.Entry is object ?
                        effectiveValue.GetValue<T>() :
                        property.GetDefaultValue(Owner.GetType());
                    ReevaluateEffectiveValue<T>(property, oldValue);
                }
                else if (effectiveValue.Priority <= BindingPriority.Animation)
                {
                    ReevaluateNonAnimatedValue(property, entry);
                }

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
                var effectiveValue = GetEffectiveValue(property);
                var entry = new BindingEntry<T>(property, source, priority);
                AddFrame(entry);

                if (priority < effectiveValue.Priority)
                {
                    var oldValue = effectiveValue.Entry is object ?
                        effectiveValue.GetValue<T>() :
                        property.GetDefaultValue(Owner.GetType());
                    ReevaluateEffectiveValue<T>(property, oldValue);
                }
                else if (effectiveValue.Priority <= BindingPriority.Animation)
                {
                    ReevaluateNonAnimatedValue(property, entry);
                }

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

        public object? GetValue(AvaloniaProperty property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var value))
                return value.GetValue();
            return GetDefaultValue(property);
        }

        public T? GetValue<T>(StyledPropertyBase<T> property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var value))
                return value.GetValue<T>();
            return property.GetDefaultValue(Owner.GetType());
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var v))
                return v.Priority <= BindingPriority.Animation;
            return false;
        }

        public bool IsSet(AvaloniaProperty property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var v))
                return v.Priority < BindingPriority.Inherited;
            return false;
        }

        public Optional<T> GetBaseValue<T>(
            StyledPropertyBase<T> property,
            BindingPriority minPriority,
            BindingPriority maxPriority)
        {
            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];

                if (frame.Priority < maxPriority || frame.Priority > minPriority)
                    continue;

                var values = frame.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];

                    if (value.Property == property &&
                        value is IValueEntry<T> typed &&
                        typed.TryGetValue(out var result))
                    {
                        return result;
                    }
                }
            }

            return default;
        }

        public void InheritanceParentChanged(ValueStore? newParent)
        {
            var newParentFrame = newParent?.GetInheritanceFrame();

            // If we don't have an inheritance frame, or we're not the owner of the inheritance
            // frame we can directly use the parent inheritance frame. Otherwise we need to
            // reparent the existing inheritance frame.
            if (_inheritanceFrame is null || _inheritanceFrame.Owner != this)
                SetInheritanceFrame(newParentFrame);
            else
                _inheritanceFrame.SetParent(newParentFrame);

            ReevaluateEffectiveValues();
        }

        /// <summary>
        /// Called by an <see cref="IValueEntry"/> to notify the value store that its value has changed.
        /// </summary>
        /// <param name="frame">The frame that the value belongs to.</param>
        /// <param name="value">The value entry.</param>
        /// <param name="oldValue">The old value of the value entry.</param>
        public void ValueChanged(
            IValueFrame frame,
            IValueEntry value,
            object? oldValue)
        {
            var property = value.Property;
            var effective = GetEffectiveValue(property);

            // Check if the changed value has higher or equal priority to the effective value.
            if (frame.Priority <= effective.Priority)
            {
                // If the changed value is not the effective value then the oldValue passed to us is of
                // no interest; we need to use the current effective value as the old value.
                if (effective.Entry != value)
                    oldValue = effective.Entry is object ?
                        effective.GetValue() :
                        GetDefaultValue(property);

                // Reevaluate the effective value.
                ReevaluateEffectiveValue(property, oldValue);
            }
            else if (effective.Priority == BindingPriority.Animation)
            {
                // The changed value is lower priority than the effective value but the effective value
                // is an animation: in this case we need to raise a non-effective value change
                // notification in order for transitions to work.
                ReevaluateNonAnimatedValue(property, value);
            }
        }

        /// <summary>
        /// Called by an <see cref="IValueEntry{T}"/> to notify the value store that its value has changed.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="frame">The frame that the value belongs to.</param>
        /// <param name="value">The value entry.</param>
        /// <param name="oldValue">The old value of the value entry.</param>
        public void ValueChanged<T>(
            IValueFrame frame,
            IValueEntry<T> value,
            Optional<T> oldValue)
        {
            var property = value.Property;
            var effective = GetEffectiveValue(property);

            // Check if the changed value has higher or equal priority than the effective value.
            if (frame.Priority <= effective.Priority)
            {
                // If the changed value is not the effective value then the oldValue passed to us is of
                // no interest; we need to use the current effective value as the old value.
                if (effective.Entry != value)
                    oldValue = effective.Entry is object ?
                        effective.GetValue<T>() :
                        property.GetDefaultValue(Owner.GetType());

                // Reevaluate the effective value.
                ReevaluateEffectiveValue(property, oldValue);
            }
            else if (effective.Priority == BindingPriority.Animation)
            {
                // The changed value is lower priority than the effective value but the effective value
                // is an animation: in this case we need to raise a non-effective value change
                // notification in order for transitions to work.
                ReevaluateNonAnimatedValue(property, value);
            }
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

        private InheritanceFrame GetInheritanceFrame()
        {
            if (_inheritanceFrame is null)
            {
                _inheritanceFrame = new(this);

                if (_effectiveValues is object)
                {
                    foreach (var (_, v) in _effectiveValues)
                    {
                        if (v.Entry!.Property.Inherits)
                            _inheritanceFrame.SetValue(v.Entry);
                    }
                }

                AddFrame(_inheritanceFrame);
            }

            return _inheritanceFrame;
        }

        private void ReevaluateEffectiveValue(AvaloniaProperty property, object? oldValue)
        {
            var newValue = AvaloniaProperty.UnsetValue;

            if (EvaluateEffectiveValue(property, out var value, out var priority))
                newValue = SetEffectiveValue(property, value, priority);
            else
                ClearEffectiveValue(property);

            RaisePropertyChanged(property, value, oldValue, newValue, priority, true);
        }

        private void ReevaluateEffectiveValue<T>(StyledPropertyBase<T> property, in Optional<T> oldValue)
        {
            Optional<T> newValue = default;

            if (EvaluateEffectiveValue(property, out var value, out var priority))
                newValue = SetEffectiveValue(property, value, priority);
            else
                ClearEffectiveValue(property);

            RaisePropertyChanged(property, value, oldValue, newValue, priority, true);
        }

        private void ReevaluateNonAnimatedValue(AvaloniaProperty property, IValueEntry changed)
        {
            if (EvaluateEffectiveValue(property, out var effective, out var priority, BindingPriority.LocalValue))
            {
                if (effective == changed)
                {
                    _nonAnimatedValues ??= new Dictionary<int, EffectiveValue>();
                    _nonAnimatedValues.Add(property.Id, new(effective, priority));
                    effective.TryGetValue(out var newValue);
                    RaisePropertyChanged(property, changed, AvaloniaProperty.UnsetValue, newValue, priority, false);
                }
            }
            else
            {
                _nonAnimatedValues?.Remove(property.Id);
                var newValue = GetDefaultValue(property);
                RaisePropertyChanged(property, changed, AvaloniaProperty.UnsetValue, newValue, priority, false);
            }
        }

        private void ReevaluateNonAnimatedValue<T>(StyledPropertyBase<T> property, IValueEntry changed)
        {
            if (EvaluateEffectiveValue(property, out var effective, out var priority, BindingPriority.LocalValue))
            {
                if (effective == changed)
                {
                    _nonAnimatedValues ??= new Dictionary<int, EffectiveValue>();
                    _nonAnimatedValues.Add(property.Id, new(effective, priority));
                    ((IValueEntry<T>)effective).TryGetValue(out var newValue);
                    RaisePropertyChanged<T>(property, changed, default, newValue, priority, false);
                }
            }
            else
            {
                _nonAnimatedValues?.Remove(property.Id);
                var newValue = property.GetDefaultValue(Owner.GetType());
                RaisePropertyChanged<T>(property, changed, default, newValue, priority, false);
            }
        }

        private bool EvaluateEffectiveValue(
            AvaloniaProperty property,
            [NotNullWhen(true)] out IValueEntry? result,
            out BindingPriority priority,
            BindingPriority maxPriority = BindingPriority.Animation)
        {
            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];

                if (!frame.IsActive || frame.Priority < maxPriority)
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

            if (property.Inherits)
            {
                var frame = _inheritanceFrame;

                if (frame?.Owner == this)
                    frame = frame.Parent;

                while (frame is object)
                {
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

                    frame = frame.Parent;
                }
            }

            result = default;
            priority = BindingPriority.Unset;
            return false;
        }

        private void ReevaluateEffectiveValues()
        {
            var newValues = DictionaryPool<int, EffectiveValue>.Get();
            Dictionary<int, EffectiveValue>? nonAnimatedValues = null;

            for (var i = _frames.Count - 1; i >= 0; --i)
            {
                var frame = _frames[i];

                if (!frame.IsActive)
                    continue;

                var values = frame.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];
                    var propertyId = value.Property.Id;

                    if (value.HasValue)
                    {
                        if (!newValues.ContainsKey(propertyId))
                        {
                            newValues.Add(propertyId, new(value, frame.Priority));

                            if (frame.Priority <= BindingPriority.Animation)
                            {
                                nonAnimatedValues ??= DictionaryPool<int, EffectiveValue>.Get();
                                nonAnimatedValues.Add(propertyId, new(null, BindingPriority.Unset));
                            }
                        }
                        else if (nonAnimatedValues is object &&
                            nonAnimatedValues.TryGetValue(propertyId, out var na) &&
                            na.Entry is null)
                        {
                            nonAnimatedValues[propertyId] = new(value, frame.Priority);
                        }
                    }
                }
            }

            var iframe = _inheritanceFrame;

            if (iframe?.Owner == this)
                iframe = iframe.Parent;

            while (iframe is object)
            {
                var values = iframe.Values;

                for (var j = 0; j < values.Count; ++j)
                {
                    var value = values[j];

                    if (!newValues.ContainsKey(value.Property.Id) && value.HasValue)
                    {
                        newValues.Add(value.Property.Id, new(value, BindingPriority.Inherited));
                    }
                }

                iframe = iframe.Parent;
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

                    RaisePropertyChanged(
                        property, 
                        newValueEntry.Entry, 
                        oldValue, 
                        newValue, 
                        newValueEntry.Priority,
                        true);

                    if (newValueEntry.Priority <= BindingPriority.Animation)
                        ReevaluateNonAnimatedValue(property, newValueEntry.Entry!);
                }
                else
                {
                    // TODO: Log error. Non-registered property changed.
                }

                oldValues?.Remove(id);
            }

            if (nonAnimatedValues is object)
            {
                foreach (var (id, value) in nonAnimatedValues)
                {
                    var property = registry.FindRegistered(id);

                    if (property is object)
                    {
                        var oldValue = AvaloniaProperty.UnsetValue;

                        if (_nonAnimatedValues is object &&
                            _nonAnimatedValues.TryGetValue(id, out var na) &&
                            na.Entry is object)
                        {
                            oldValue = na.GetValue();
                        }

                        var newValue = value.Entry is object ?
                            value.GetValue() :
                            GetDefaultValue(property);

                        RaisePropertyChanged(
                            property,
                            value.Entry,
                            oldValue,
                            newValue,
                            value.Priority,
                            false);
                    }
                    else
                    {
                        // TODO: Log error. Non-registered property changed.
                    }
                }
            }

            if (_nonAnimatedValues is object)
                DictionaryPool<int, EffectiveValue>.Release(_nonAnimatedValues);
            _nonAnimatedValues = nonAnimatedValues;

            if (oldValues is object)
            {
                foreach (var (id, oldValueEntry) in oldValues)
                {
                    var property = registry.FindRegistered(id);

                    if (property is object)
                    {
                        var newValue = AvaloniaProperty.UnsetValue;
                        var oldValue = oldValueEntry.GetValue();
                        RaisePropertyChanged(property, null, oldValue, newValue, BindingPriority.Unset, true);
                    }
                    else
                    {
                        // TODO: Log error. Non-registered property changed.
                    }
                }

                DictionaryPool<int, EffectiveValue>.Release(oldValues);
            }
        }

        private EffectiveValue GetEffectiveValue(AvaloniaProperty property)
        {
            if (_effectiveValues is object && _effectiveValues.TryGetValue(property.Id, out var value))
                return value;
            return new EffectiveValue(null!, BindingPriority.Unset);
        }

        private object? SetEffectiveValue(AvaloniaProperty property, IValueEntry value, BindingPriority priority)
        {
            _effectiveValues ??= new();
            _effectiveValues[property.Id] = new(value, priority);

            if (priority > BindingPriority.Animation)
                _nonAnimatedValues?.Remove(property.Id);

            value.TryGetValue(out var result);
            return result;
        }

        private T? SetEffectiveValue<T>(StyledPropertyBase<T> property, IValueEntry value, BindingPriority priority)
        {
            _effectiveValues ??= new();
            _effectiveValues[property.Id] = new(value, priority);

            if (priority > BindingPriority.Animation)
                _nonAnimatedValues?.Remove(property.Id);

            ((IValueEntry<T>)value).TryGetValue(out var result);
            return result;
        }

        private void ClearEffectiveValue(AvaloniaProperty property)
        {
            _effectiveValues?.Remove(property.Id);
            _nonAnimatedValues?.Remove(property.Id);
        }

        private void SetInheritanceFrame(InheritanceFrame? frame)
        {
            _inheritanceFrame = frame;

            var childCount = Owner.GetInheritanceChildCount();

            for (var i = 0; i < childCount; ++i)
            {
                var child = Owner.GetInheritanceChild(i);
                child.GetValueStore().ParentInheritanceFrameChanged(frame);
            }
        }

        private void SetInheritanceFrameValue(IValueEntry entry)
        {
            var frame = _inheritanceFrame!;

            if (frame.Owner != this)
                frame = new InheritanceFrame(this, _inheritanceFrame);

            frame.SetValue(entry);
            SetInheritanceFrame(frame);
        }

        private void ParentInheritanceFrameChanged(InheritanceFrame? frame)
        {
            if (_inheritanceFrame?.Owner == this)
                _inheritanceFrame.SetParent(frame);
            else
                SetInheritanceFrame(frame);
        }

        private void InheritedValueChanged(AvaloniaProperty property)
        {
            // If the inherited value is set locally, propagation stops here.
            if (_inheritanceFrame!.Owner == this && _inheritanceFrame.TryGet(property, out _))
                return;

            ReevaluateEffectiveValue(property, GetValue(property));
            NotifyChildrenInheritedValueChanged(property);
        }

        private void InheritedValueChanged<T>(StyledPropertyBase<T> property)
        {
            // If the inherited value is set locally, propagation stops here.
            if (_inheritanceFrame!.Owner == this && _inheritanceFrame.TryGet(property, out _))
                return;

            ReevaluateEffectiveValue<T>(property, GetValue(property));
            NotifyChildrenInheritedValueChanged(property);
        }

        private void NotifyChildrenInheritedValueChanged(AvaloniaProperty property)
        {
            var childCount = Owner.GetInheritanceChildCount();

            for (var i = 0; i < childCount; ++i)
            {
                var child = Owner.GetInheritanceChild(i);
                child.GetValueStore().InheritedValueChanged(property);
            }
        }

        private void NotifyChildrenInheritedValueChanged<T>(StyledPropertyBase<T> property)
        {
            var childCount = Owner.GetInheritanceChildCount();

            for (var i = 0; i < childCount; ++i)
            {
                var child = Owner.GetInheritanceChild(i);
                child.GetValueStore().InheritedValueChanged(property);
            }
        }

        private object? GetDefaultValue(AvaloniaProperty property)
        {
            return ((IStyledPropertyAccessor)property).GetDefaultValue(Owner.GetType());
        }

        private void RaisePropertyChanged(
            AvaloniaProperty property,
            IValueEntry? entry,
            object? oldValue,
            object? newValue,
            BindingPriority priority,
            bool isEffectiveValueChange)
        {
            var raiseInherited = false;

            if (priority < BindingPriority.Inherited &&
                entry is object &&
                _inheritanceFrame is object &&
                property.Inherits)
            {
                SetInheritanceFrameValue(entry);
                raiseInherited = true;
            }

            if (oldValue == AvaloniaProperty.UnsetValue)
                oldValue = GetDefaultValue(property);
            if (newValue == AvaloniaProperty.UnsetValue)
                newValue = GetDefaultValue(property);

            if (!Equals(oldValue, newValue))
                property.RaisePropertyChanged(Owner, oldValue, newValue, priority, isEffectiveValueChange);
            if (raiseInherited)
                NotifyChildrenInheritedValueChanged(property);
        }

        private void RaisePropertyChanged<T>(
            StyledPropertyBase<T> property,
            IValueEntry? entry,
            Optional<T> oldValue,
            Optional<T> newValue,
            BindingPriority priority,
            bool isEffectiveValueChange)
        {
            var raiseInherited = false;

            if (priority < BindingPriority.Inherited &&
                entry is object &&
                _inheritanceFrame is object &&
                property.Inherits)
            {
                SetInheritanceFrameValue(entry);
                raiseInherited = true;
            }

            if (isEffectiveValueChange)
            {
                if (!oldValue.HasValue)
                    oldValue = property.GetDefaultValue(Owner.GetType());
                if (!newValue.HasValue)
                    newValue = property.GetDefaultValue(Owner.GetType());
            }

            if (oldValue != newValue || !isEffectiveValueChange)
                Owner.RaisePropertyChanged(property, oldValue, newValue, priority, isEffectiveValueChange);
            if (raiseInherited)
                NotifyChildrenInheritedValueChanged(property);
        }

        private readonly struct EffectiveValue
        {
            public EffectiveValue(IValueEntry? value, BindingPriority priority)
            {
                Entry = value;
                Priority = priority;
            }

            public readonly IValueEntry? Entry;
            public readonly BindingPriority Priority;

            public object? GetValue()
            {
                Entry!.TryGetValue(out var result);
                return result;
            }

            public T? GetValue<T>()
            {
                if (Entry is IValueEntry<T> typed)
                {
                    typed.TryGetValue(out var result);
                    return result;
                }
                else
                {
                    Entry!.TryGetValue(out var result);
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
