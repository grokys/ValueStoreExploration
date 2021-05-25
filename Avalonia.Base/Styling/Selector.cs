using System;

#nullable enable

namespace Avalonia.Styling
{
    /// <summary>
    /// A selector in a <see cref="Style"/>.
    /// </summary>
    public abstract class Selector
    {
        /// <summary>
        /// Gets a value indicating whether either this selector or a previous selector has moved
        /// into a template.
        /// </summary>
        public abstract bool InTemplate { get; }

        /// <summary>
        /// Gets a value indicating whether this selector is a combinator.
        /// </summary>
        /// <remarks>
        /// A combinator is a selector such as Child or Descendent which links simple selectors.
        /// </remarks>
        public abstract bool IsCombinator { get; }

        /// <summary>
        /// Gets the target type of the selector, if available.
        /// </summary>
        public abstract Type? TargetType { get; }

        /// <summary>
        /// Moves to the previous selector.
        /// </summary>
        protected abstract Selector? MovePrevious();

        /// <summary>
        /// Evaluates the selector for a match.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="subscribe">
        /// Whether the match should subscribe to changes in order to track the match over time,
        /// or simply return an immediate result.
        /// </param>
        /// <returns>A <see cref="SelectorMatch"/>.</returns>
        internal abstract SelectorMatch Evaluate(IStyleable control, bool subscribe);
    }
}
