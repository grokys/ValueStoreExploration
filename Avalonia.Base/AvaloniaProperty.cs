using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia
{
    /// <summary>
    /// Base class for avalonia properties.
    /// </summary>
    public abstract class AvaloniaProperty : IEquatable<AvaloniaProperty>
    {
        /// <summary>
        /// Represents an unset property value.
        /// </summary>
        public static readonly object UnsetValue = new UnsetValueType();

        private static int s_nextId;
        private readonly AvaloniaPropertyMetadata _defaultMetadata;
        private readonly Dictionary<Type, AvaloniaPropertyMetadata> _metadata;
        private readonly Dictionary<Type, AvaloniaPropertyMetadata> _metadataCache = new Dictionary<Type, AvaloniaPropertyMetadata>();

        private bool _hasMetadataOverrides;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaProperty"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="valueType">The type of the property's value.</param>
        /// <param name="ownerType">The type of the class that registers the property.</param>
        /// <param name="metadata">The property metadata.</param>
        protected AvaloniaProperty(
            string name,
            Type valueType,
            Type ownerType,
            AvaloniaPropertyMetadata metadata)
        {
            if (name.Contains("."))
            {
                throw new ArgumentException("'name' may not contain periods.");
            }

            _metadata = new Dictionary<Type, AvaloniaPropertyMetadata>();

            Name = name;
            PropertyType = valueType;
            OwnerType = ownerType;
            Id = s_nextId++;

            _metadata.Add(ownerType, metadata);
            _defaultMetadata = metadata;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaProperty"/> class.
        /// </summary>
        /// <param name="source">The direct property to copy.</param>
        /// <param name="ownerType">The new owner type.</param>
        /// <param name="metadata">Optional overridden metadata.</param>
        protected AvaloniaProperty(
            AvaloniaProperty source,
            Type ownerType,
            AvaloniaPropertyMetadata metadata)
        {
            _metadata = new Dictionary<Type, AvaloniaPropertyMetadata>();

            Name = source.Name;
            PropertyType = source.PropertyType;
            OwnerType = ownerType;
            Id = source.Id;
            _defaultMetadata = source._defaultMetadata;

            // Properties that have different owner can't use fast path for metadata.
            _hasMetadataOverrides = true;

            if (metadata != null)
            {
                _metadata.Add(ownerType, metadata);
            }
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the property's value.
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the type of the class that registered the property.
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// Gets a value indicating whether the property inherits its value.
        /// </summary>
        public virtual bool Inherits => false;

        /// <summary>
        /// Gets a value indicating whether this is an attached property.
        /// </summary>
        public virtual bool IsAttached => false;

        /// <summary>
        /// Gets a value indicating whether this is a direct property.
        /// </summary>
        public virtual bool IsDirect => false;

        /// <summary>
        /// Gets a value indicating whether this is a readonly property.
        /// </summary>
        public virtual bool IsReadOnly => false;

        /// <summary>
        /// Gets the integer ID that represents this property.
        /// </summary>
        internal int Id { get; }

        /// <summary>
        /// Tests two <see cref="AvaloniaProperty"/>s for equality.
        /// </summary>
        /// <param name="a">The first property.</param>
        /// <param name="b">The second property.</param>
        /// <returns>True if the properties are equal, otherwise false.</returns>
        public static bool operator ==(AvaloniaProperty a, AvaloniaProperty b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            else if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            else
            {
                return a.Equals(b);
            }
        }

        /// <summary>
        /// Tests two <see cref="AvaloniaProperty"/>s for inequality.
        /// </summary>
        /// <param name="a">The first property.</param>
        /// <param name="b">The second property.</param>
        /// <returns>True if the properties are equal, otherwise false.</returns>
        public static bool operator !=(AvaloniaProperty a, AvaloniaProperty b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Registers a <see cref="AvaloniaProperty"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type of the class that is registering the property.</typeparam>
        /// <typeparam name="TValue">The type of the property's value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="inherits">Whether the property inherits its value.</param>
        /// <param name="defaultBindingMode">The default binding mode for the property.</param>
         /// <param name="validate">A value validation callback.</param>
        /// <param name="coerce">A value coercion callback.</param>
        /// <returns>A <see cref="StyledProperty{TValue}"/></returns>
        public static StyledProperty<TValue> Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default(TValue),
            bool inherits = false,
            BindingMode defaultBindingMode = BindingMode.OneWay,
            Func<TValue, bool> validate = null,
            Func<IAvaloniaObject, TValue, TValue> coerce = null)
                where TOwner : IAvaloniaObject
        {
            var metadata = new StyledPropertyMetadata<TValue>(
                defaultValue,
                defaultBindingMode: defaultBindingMode,
                coerce: coerce);

            var result = new StyledProperty<TValue>(
                name,
                typeof(TOwner),
                metadata,
                inherits,
                validate);
            AvaloniaPropertyRegistry.Instance.Register(typeof(TOwner), result);
            return result;
        }

        /// <summary>
        /// Registers an attached <see cref="AvaloniaProperty"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type of the class that is registering the property.</typeparam>
        /// <typeparam name="THost">The type of the class that the property is to be registered on.</typeparam>
        /// <typeparam name="TValue">The type of the property's value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="inherits">Whether the property inherits its value.</param>
        /// <param name="defaultBindingMode">The default binding mode for the property.</param>
        /// <param name="validate">A value validation callback.</param>
        /// <param name="coerce">A value coercion callback.</param>
        /// <returns>A <see cref="AvaloniaProperty{TValue}"/></returns>
        public static AttachedProperty<TValue> RegisterAttached<TOwner, THost, TValue>(
            string name,
            TValue defaultValue = default(TValue),
            bool inherits = false,
            BindingMode defaultBindingMode = BindingMode.OneWay,
            Func<TValue, bool> validate = null,
            Func<IAvaloniaObject, TValue, TValue> coerce = null)
                where THost : IAvaloniaObject
        {
            var metadata = new StyledPropertyMetadata<TValue>(
                defaultValue,
                defaultBindingMode: defaultBindingMode,
                coerce: coerce);

            var result = new AttachedProperty<TValue>(name, typeof(TOwner), metadata, inherits, validate);
            var registry = AvaloniaPropertyRegistry.Instance;
            registry.Register(typeof(TOwner), result);
            registry.RegisterAttached(typeof(THost), result);
            return result;
        }

        /// <summary>
        /// Registers an attached <see cref="AvaloniaProperty"/>.
        /// </summary>
        /// <typeparam name="THost">The type of the class that the property is to be registered on.</typeparam>
        /// <typeparam name="TValue">The type of the property's value.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="ownerType">The type of the class that is registering the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="inherits">Whether the property inherits its value.</param>
        /// <param name="defaultBindingMode">The default binding mode for the property.</param>
        /// <param name="validate">A value validation callback.</param>
        /// <param name="coerce">A value coercion callback.</param>
        /// <returns>A <see cref="AvaloniaProperty{TValue}"/></returns>
        public static AttachedProperty<TValue> RegisterAttached<THost, TValue>(
            string name,
            Type ownerType,
            TValue defaultValue = default(TValue),
            bool inherits = false,
            BindingMode defaultBindingMode = BindingMode.OneWay,
            Func<TValue, bool> validate = null,
            Func<IAvaloniaObject, TValue, TValue> coerce = null)
                where THost : IAvaloniaObject
        {
            var metadata = new StyledPropertyMetadata<TValue>(
                defaultValue,
                defaultBindingMode: defaultBindingMode,
                coerce: coerce);

            var result = new AttachedProperty<TValue>(name, ownerType, metadata, inherits, validate);
            var registry = AvaloniaPropertyRegistry.Instance;
            registry.Register(ownerType, result);
            registry.RegisterAttached(typeof(THost), result);
            return result;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var p = obj as AvaloniaProperty;
            return p != null && Equals(p);
        }

        /// <inheritdoc/>
        public bool Equals(AvaloniaProperty other)
        {
            return other != null && Id == other.Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Gets the property metadata for the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>
        /// The property metadata.
        /// </returns>
        public AvaloniaPropertyMetadata GetMetadata<T>() where T : IAvaloniaObject
        {
            return GetMetadata(typeof(T));
        }

        /// <summary>
        /// Gets the property metadata for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The property metadata.
        /// </returns>
        ///
        public AvaloniaPropertyMetadata GetMetadata(Type type)
        {
            if (!_hasMetadataOverrides)
            {
                return _defaultMetadata;
            }

            return GetMetadataWithOverrides(type);
        }

        /// <summary>
        /// Checks whether the <paramref name="value"/> is valid for the property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the value is valid, otherwise false.</returns>
        public bool IsValidValue(object value)
        {
            return TypeUtilities.TryConvertImplicit(PropertyType, value, out value);
        }

        /// <summary>
        /// Gets the string representation of the property.
        /// </summary>
        /// <returns>The property's string representation.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Uses the visitor pattern to resolve an untyped property to a typed property.
        /// </summary>
        /// <typeparam name="TData">The type of user data passed.</typeparam>
        /// <param name="vistor">The visitor which will accept the typed property.</param>
        /// <param name="data">The user data to pass.</param>
        public abstract void Accept<TData>(IAvaloniaPropertyVisitor<TData> vistor, ref TData data)
            where TData : struct;

        /// <summary>
        /// Overrides the metadata for the property on the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="metadata">The metadata.</param>
        protected void OverrideMetadata(Type type, AvaloniaPropertyMetadata metadata)
        {
            if (_metadata.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    $"Metadata is already set for {Name} on {type}.");
            }

            var baseMetadata = GetMetadata(type);
            metadata.Merge(baseMetadata, this);
            _metadata.Add(type, metadata);
            _metadataCache.Clear();

            _hasMetadataOverrides = true;
        }

        private AvaloniaPropertyMetadata GetMetadataWithOverrides(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_metadataCache.TryGetValue(type, out AvaloniaPropertyMetadata result))
            {
                return result;
            }

            Type currentType = type;

            while (currentType != null)
            {
                if (_metadata.TryGetValue(currentType, out result))
                {
                    _metadataCache[type] = result;

                    return result;
                }

                currentType = currentType.BaseType;
            }

            _metadataCache[type] = _defaultMetadata;

            return _defaultMetadata;
        }
    }

    /// <summary>
    /// Class representing the <see cref="AvaloniaProperty.UnsetValue"/>.
    /// </summary>
    public sealed class UnsetValueType
    {
        internal UnsetValueType() { }

        /// <summary>
        /// Returns the string representation of the <see cref="AvaloniaProperty.UnsetValue"/>.
        /// </summary>
        /// <returns>The string "(unset)".</returns>
        public override string ToString() => "(unset)";
    }
}
