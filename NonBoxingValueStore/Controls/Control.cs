using System;
using Avalonia.Collections;
using Avalonia.PropertyStore;
using Avalonia.Styling;

namespace Avalonia.Controls
{
    public class Control : AvaloniaObject, IStyleable
    {
        private AvaloniaList<string>? _classes;

        public AvaloniaList<string> Classes => _classes ??= new AvaloniaList<string>();
        IAvaloniaReadOnlyList<string> IStyleable.Classes => Classes;

        public Type StyleKey => GetType();

        public IAvaloniaObject? TemplatedParent => null;

        public string? Name { get; set; }

        void IStyleable.ApplyStyle(IStyle style)
        {
            if ((style as Style)?.Instance(this) is IValueFrame frame)
                ApplyStyle(frame);
        }

        void IStyleable.BeginStyling() => BeginStyling();
        void IStyleable.EndStyling() => EndStyling();
    }
}
