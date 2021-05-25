#nullable enable

namespace Avalonia.PropertyStore
{
    /// <summary>
    /// Represents a typed value entry in an <see cref="IValueFrame"/>.
    /// </summary>
    internal interface IValue<T> : IValue
    {
        /// <summary>
        /// Tries to get the value associated with the entry.
        /// </summary>
        /// <param name="value">
        /// When this method returns, contains the value associated with the entry if a value is
        /// present; otherwise, returns the default value of <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// true if the entry has an associated value; otherwise false.
        /// </returns>
        bool TryGetValue(out T? value);
    }
}
