using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia
{
    /// <summary>
    /// Base class for styled properties.
    /// </summary>
    public abstract class StyledPropertyBase<TValue> : AvaloniaProperty<TValue>, IStyledPropertyAccessor
    {
        private bool _inherits;

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledPropertyBase{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="ownerType">The type of the class that registers the property.</param>
        /// <param name="metadata">The property metadata.</param>
        /// <param name="inherits">Whether the property inherits its value.</param>
        /// <param name="validate">A value validation callback.</param>
        /// <param name="notifying">A <see cref="AvaloniaProperty.Notifying"/> callback.</param>
        protected StyledPropertyBase(
            string name,
            Type ownerType,            
            StyledPropertyMetadata<TValue> metadata,
            bool inherits = false,
            Func<TValue, bool> validate = null)
                : base(name, ownerType, metadata)
        {
            if (name.Contains("."))
            {
                throw new ArgumentException("'name' may not contain periods.");
            }

            _inherits = inherits;
            ValidateValue = validate;
            HasCoercion |= metadata.CoerceValue != null;

            if (validate?.Invoke(metadata.DefaultValue) == false)
            {
                throw new ArgumentException(
                    $"'{metadata.DefaultValue}' is not a valid default value for '{name}'.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledPropertyBase{T}"/> class.
        /// </summary>
        /// <param name="source">The property to add the owner to.</param>
        /// <param name="ownerType">The type of the class that registers the property.</param>
        protected StyledPropertyBase(StyledPropertyBase<TValue> source, Type ownerType)
            : base(source, ownerType, null)
        {
            _inherits = source.Inherits;
        }

        /// <summary>
        /// Gets a value indicating whether the property inherits its value.
        /// </summary>
        /// <value>
        /// A value indicating whether the property inherits its value.
        /// </value>
        public override bool Inherits => _inherits;

        /// <summary>
        /// Gets the value validation callback for the property.
        /// </summary>
        public Func<TValue?, bool> ValidateValue { get; }

        /// <summary>
        /// Gets a value indicating whether this property has any value coercion callbacks defined
        /// in its metadata.
        /// </summary>
        internal bool HasCoercion { get; private set; }

        public TValue CoerceValue(IAvaloniaObject instance, TValue baseValue)
        {
            var metadata = GetMetadata(instance.GetType());

            if (metadata.CoerceValue != null)
            {
                return metadata.CoerceValue.Invoke(instance, baseValue);
            }

            return baseValue;
        }

        /// <summary>
        /// Gets the default value for the property on the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        public TValue GetDefaultValue(Type type)
        {
            return GetMetadata(type).DefaultValue;
        }

        /// <summary>
        /// Gets the property metadata for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The property metadata.
        /// </returns>
        public new StyledPropertyMetadata<TValue> GetMetadata(Type type)
        {
            return (StyledPropertyMetadata<TValue>)base.GetMetadata(type);
        }

        /// <summary>
        /// Overrides the default value for the property on the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="defaultValue">The default value.</param>
        public void OverrideDefaultValue<T>(TValue defaultValue) where T : IAvaloniaObject
        {
            OverrideDefaultValue(typeof(T), defaultValue);
        }

        /// <summary>
        /// Overrides the default value for the property on the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="defaultValue">The default value.</param>
        public void OverrideDefaultValue(Type type, TValue defaultValue)
        {
            OverrideMetadata(type, new StyledPropertyMetadata<TValue>(defaultValue));
        }

        /// <summary>
        /// Overrides the metadata for the property on the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="metadata">The metadata.</param>
        public void OverrideMetadata<T>(StyledPropertyMetadata<TValue> metadata) where T : IAvaloniaObject
        {
            base.OverrideMetadata(typeof(T), metadata);
        }

        /// <summary>
        /// Overrides the metadata for the property on the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="metadata">The metadata.</param>
        public void OverrideMetadata(Type type, StyledPropertyMetadata<TValue> metadata)
        {
            if (ValidateValue != null)
            {
                if (!ValidateValue(metadata.DefaultValue))
                {
                    throw new ArgumentException(
                        $"'{metadata.DefaultValue}' is not a valid default value for '{Name}'.");
                }
            }

            HasCoercion |= metadata.CoerceValue != null;

            base.OverrideMetadata(type, metadata);
        }

        /// <summary>
        /// Gets the string representation of the property.
        /// </summary>
        /// <returns>The property's string representation.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        object IStyledPropertyAccessor.GetDefaultValue(Type type) => GetDefaultBoxedValue(type);

        internal override void RaisePropertyChanged(
            AvaloniaObject owner,
            object oldValue,
            object newValue,
            BindingPriority priority,
            bool isEffectiveValueChange)
        {
            var o = oldValue != UnsetValue ? new Optional<TValue>((TValue)oldValue) : default;
            var n = newValue != UnsetValue ? new BindingValue<TValue>((TValue)newValue) : default;
            owner.RaisePropertyChanged(this, o, n, priority, isEffectiveValueChange);
        }

        private object GetDefaultBoxedValue(Type type)
        {
            return GetMetadata(type).DefaultValue;
        }

        [DebuggerHidden]
        private Func<IAvaloniaObject, TValue, TValue> Cast<THost>(Func<THost, TValue, TValue> validate)
        {
            return (o, v) => validate((THost)o, v);
        }
    }
}
