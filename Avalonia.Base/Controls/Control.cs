using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.PropertyStore;
using Avalonia.Styling;

namespace Avalonia.Controls
{
    public class Control : AvaloniaObject, IStyleable
    {
        private AvaloniaList<string>? _classes;
        private Control? _parent;
        private List<Control> _children = new();

        public AvaloniaList<string> Classes => _classes ??= new AvaloniaList<string>();
        IAvaloniaReadOnlyList<string> IStyleable.Classes => Classes;

        public Type StyleKey => GetType();
        public IAvaloniaObject? TemplatedParent => null;
        public string? Name { get; set; }

        public Control? Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != value)
                {
                    _parent?._children.Remove(this);
                    _parent = value;
                    _parent?._children.Add(this);
                    InheritanceParentChanged(value);
                }
            }
        }

        void IStyleable.ApplyStyle(IStyle style)
        {
            if ((style as Style)?.Instance(this) is IValueFrame frame)
                ApplyStyle(frame);
        }

        void IStyleable.BeginStyling() => BeginStyling();
        void IStyleable.EndStyling() => EndStyling();

        internal override int GetInheritanceChildCount() => _children.Count;
        internal override AvaloniaObject GetInheritanceChild(int index) => _children[index];
        private protected override AvaloniaObject? GetInheritanceParent => _parent;

    }
}
