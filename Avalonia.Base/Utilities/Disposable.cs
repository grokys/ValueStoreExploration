using System;

namespace Avalonia.Utilities
{
    internal static class Disposable
    {
        public static readonly IDisposable Empty = new EmptyDisposable();

        private class EmptyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
