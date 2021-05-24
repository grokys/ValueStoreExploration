using System.Collections.Generic;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    /// <summary>
    /// Represents a collection of property values in a <see cref="ValueStore"/>.
    /// </summary>
    /// <remarks>
    /// A value frame is an abstraction over the following sources of values in an
    /// <see cref="AvaloniaObject"/>:
    /// 
    /// - A style
    /// - Local values
    /// - Animation values
    /// </remarks>
    internal interface IValueFrame
    {
        /// <summary>
        /// Gets a value indicating whether the frame is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets the frame's priority.
        /// </summary>
        BindingPriority Priority { get; }

        /// <summary>
        /// Gets the frames values.
        /// </summary>
        IList<IValue> Values { get; }

        bool TryGetValue<T>(AvaloniaProperty property, out T? value);
    }
}
