using System;
using Avalonia.Data;

#nullable enable

namespace Avalonia
{
    /// <summary>
    /// Interface for getting/setting <see cref="AvaloniaProperty"/> values on an object.
    /// </summary>
    public interface IAvaloniaObject
    {
        /// <summary>
        /// Clears an <see cref="AvaloniaProperty"/>'s local value.
        /// </summary>
        /// <param name="property">The property.</param>
        void ClearValue<T>(StyledPropertyBase<T> property);

        /// <summary>
        /// Gets an observable for a <see cref="AvaloniaProperty"/>.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>
        /// An observable which fires immediately with the current value of the property on the
        /// object and subsequently each time the property value changes.
        /// </returns>
        /// <remarks>
        /// The subscription to <paramref name="o"/> is created using a weak reference.
        /// </remarks>
        IObservable<T?> GetObservable<T>(StyledPropertyBase<T> property);

        /// <summary>
        /// Gets a <see cref="AvaloniaProperty"/> value.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>The value.</returns>
        T? GetValue<T>(StyledPropertyBase<T> property);

        /// <summary>
        /// Checks whether a <see cref="AvaloniaProperty"/> is animating.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>True if the property is animating, otherwise false.</returns>
        bool IsAnimating(AvaloniaProperty property);

        /// <summary>
        /// Checks whether a <see cref="AvaloniaProperty"/> is set on this object.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>True if the property is set, otherwise false.</returns>
        bool IsSet(AvaloniaProperty property);

        /// <summary>
        /// Sets a <see cref="AvaloniaProperty"/> value.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority of the value.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> if setting the property can be undone, otherwise null.
        /// </returns>
        void SetValue<T>(StyledPropertyBase<T> property, T value);

        /// <summary>
        /// Binds a <see cref="AvaloniaProperty"/> to an observable.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="source">The observable.</param>
        /// <param name="priority">The priority of the binding.</param>
        /// <returns>
        /// A disposable which can be used to terminate the binding.
        /// </returns>
        IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority = BindingPriority.LocalValue);

        /// <summary>
        /// Binds a <see cref="AvaloniaProperty"/> to an observable.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="source">The observable.</param>
        /// <param name="priority">The priority of the binding.</param>
        /// <returns>
        /// A disposable which can be used to terminate the binding.
        /// </returns>
        IDisposable Bind<T>(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority = BindingPriority.LocalValue);

        /// <summary>
        /// Coerces the specified <see cref="AvaloniaProperty"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        void CoerceValue<T>(StyledPropertyBase<T> property);

        /// <summary>
        /// Registers an object as an inheritance child.
        /// </summary>
        /// <param name="child">The inheritance child.</param>
        /// <remarks>
        /// Inheritance children will receive a call to
        /// <see cref="InheritedPropertyChanged{T}(AvaloniaProperty{T}, Optional{T}, Optional{T})"/>
        /// when an inheritable property value changes on the parent.
        /// </remarks>
        void AddInheritanceChild(IAvaloniaObject child);

        /// <summary>
        /// Unregisters an object as an inheritance child.
        /// </summary>
        /// <param name="child">The inheritance child.</param>
        /// <remarks>
        /// Removes an inheritance child that was added by a call to
        /// <see cref="AddInheritanceChild(IAvaloniaObject)"/>.
        /// </remarks>
        void RemoveInheritanceChild(IAvaloniaObject child);
    }
}
