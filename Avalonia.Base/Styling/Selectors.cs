using System;

namespace Avalonia.Styling
{
    /// <summary>
    /// Extension methods for <see cref="Selector"/>.
    /// </summary>
    public static class Selectors
    {
        /// <summary>
        /// Returns a selector which matches a control's style class.
        /// </summary>
        /// <param name="previous">The previous selector.</param>
        /// <param name="name">The name of the style class.</param>
        /// <returns>The selector.</returns>
        public static Selector Class(this Selector previous, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Class name may not be null or empty", nameof(name));

            var tac = previous as TypeNameAndClassSelector;

            if (tac != null)
            {
                tac.Classes.Add(name);
                return tac;
            }
            else
            {
                return TypeNameAndClassSelector.ForClass(previous, name);
            }
        }

        /// <summary>
        /// Returns a selector which matches a type or a derived type.
        /// </summary>
        /// <param name="previous">The previous selector.</param>
        /// <param name="type">The type.</param>
        /// <returns>The selector.</returns>
        public static Selector Is(this Selector previous, Type type)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            return TypeNameAndClassSelector.Is(previous, type);
        }

        /// <summary>
        /// Returns a selector which matches a type or a derived type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="previous">The previous selector.</param>
        /// <returns>The selector.</returns>
        public static Selector Is<T>(this Selector previous) where T : IStyleable
        {
            return previous.Is(typeof(T));
        }

        /// <summary>
        /// Returns a selector which matches a control's Name.
        /// </summary>
        /// <param name="previous">The previous selector.</param>
        /// <param name="name">The name.</param>
        /// <returns>The selector.</returns>
        public static Selector Name(this Selector previous, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Control name may not be null or empty", nameof(name));

            var tac = previous as TypeNameAndClassSelector;

            if (tac != null)
            {
                tac.Name = name;
                return tac;
            }
            else
            {
                return TypeNameAndClassSelector.ForName(previous, name);
            }
        }

        /// <summary>
        /// Returns a selector which matches a type.
        /// </summary>
        /// <param name="previous">The previous selector.</param>
        /// <param name="type">The type.</param>
        /// <returns>The selector.</returns>
        public static Selector OfType(this Selector previous, Type type)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            return TypeNameAndClassSelector.OfType(previous, type);
        }

        /// <summary>
        /// Returns a selector which matches a type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="previous">The previous selector.</param>
        /// <returns>The selector.</returns>
        public static Selector OfType<T>(this Selector previous) where T : IStyleable
        {
            return previous.OfType(typeof(T));
        }
    }
}
