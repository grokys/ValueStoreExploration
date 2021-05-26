using System;

namespace Avalonia.Data
{
    /// <summary>
    /// Holds the result of calling <see cref="IBinding.Initiate"/>.
    /// </summary>
    /// <remarks>
    /// Whereas an <see cref="IBinding"/> holds a description of a binding such as "Bind to the X
    /// property on a control's DataContext"; this class represents a binding that has been 
    /// *instanced* by calling <see cref="IBinding.Initiate(IAvaloniaObject, AvaloniaProperty, object, bool)"/>
    /// on a target object.
    /// </remarks>
    public class InstancedBinding
    {
        public InstancedBinding(IObservable<object> source, BindingMode mode, BindingPriority priority)
        {
            Mode = mode;
            Priority = priority;
            Observable = source;
        }

        /// <summary>
        /// Gets the binding mode with which the binding was initiated.
        /// </summary>
        public BindingMode Mode { get; }

        /// <summary>
        /// Gets the binding priority.
        /// </summary>
        public BindingPriority Priority { get; }

        /// <summary>
        /// Gets the <see cref="Value"/> as an observable.
        /// </summary>
        public IObservable<object> Observable { get; }
    }
}
