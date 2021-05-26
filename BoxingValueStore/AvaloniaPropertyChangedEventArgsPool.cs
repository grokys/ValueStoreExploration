using System.Collections.Generic;
using Avalonia.Data;

namespace Avalonia
{
    internal static class AvaloniaPropertyChangedEventArgsPool
    {
        private const int MaxPoolSize = 4;
        private static Stack<AvaloniaPropertyChangedEventArgs> _pool = new();

        public static AvaloniaPropertyChangedEventArgs Get(
            IAvaloniaObject sender,
            AvaloniaProperty property,
            object? oldValue,
            object? newValue,
            BindingPriority priority)
        {
            if (_pool.Count == 0)
            {
                return new(sender, property, oldValue, newValue, priority);
            }
            else
            {
                var e = _pool.Pop();
                e.Initialize(sender, property, oldValue, newValue, priority);
                return e;
            }
        }

        public static void Release(AvaloniaPropertyChangedEventArgs e)
        {
            if (_pool.Count < MaxPoolSize)
            {
                e.Recycle();
                _pool.Push(e);
            }
        }
    }
}
