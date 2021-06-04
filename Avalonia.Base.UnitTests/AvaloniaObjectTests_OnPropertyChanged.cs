// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_OnPropertyChanged
    {
        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Property_Change()
        {
            var target = new Class1();

            target.SetValue(Class1.FooProperty, "newvalue");

            Assert.Equal(1, target.CoreChanges.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[0];

            Assert.Equal("newvalue", change.NewValue.Value);
            Assert.Equal("foodefault", change.OldValue.Value);
            Assert.Equal(BindingPriority.LocalValue, change.Priority);
            Assert.True(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Local_Value_Change()
        {
            var target = new Class1();
            var source = new BehaviorSubject<BindingValue<string>>("animation");

            target.Bind(Class1.FooProperty, source, BindingPriority.Animation);
            target.SetValue(Class1.FooProperty, "localvalue");

            Assert.Equal(2, target.CoreChanges.Count);
            Assert.Equal(1, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[1];

            Assert.Equal("localvalue", change.NewValue.Value);
            Assert.False(change.OldValue.HasValue);
            Assert.Equal(BindingPriority.LocalValue, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Local_Binding_Change()
        {
            var target = new Class1();
            var source1 = new BehaviorSubject<BindingValue<string>>("animation");
            var source2 = new BehaviorSubject<BindingValue<string>>("localvalue");

            target.Bind(Class1.FooProperty, source1, BindingPriority.Animation);
            target.Bind(Class1.FooProperty, source2);

            Assert.Equal(2, target.CoreChanges.Count);
            Assert.Equal(1, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[1];

            Assert.Equal("localvalue", change.NewValue.Value);
            Assert.False(change.OldValue.HasValue);
            Assert.Equal(BindingPriority.LocalValue, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Local_Value_Clear()
        {
            var target = new Class1();
            var source = new BehaviorSubject<BindingValue<string>>("animation");

            target.Bind(Class1.FooProperty, source, BindingPriority.Animation);
            target.SetValue(Class1.FooProperty, "localvalue");
            target.CoreChanges.Clear();
            target.Changes.Clear();
            target.ClearValue(Class1.FooProperty);

            Assert.Equal(1, target.CoreChanges.Count);
            Assert.Equal(0, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[0];

            Assert.Equal("foodefault", change.NewValue.Value);
            Assert.False(change.OldValue.HasValue);
            Assert.Equal(BindingPriority.Unset, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Style_Value_Change()
        {
            var target = new Class1();
            var source = new BehaviorSubject<BindingValue<string>>("animation");
            var style = new Style(x => x.OfType<Class1>().Class("foo"))
            {
                Setters = { new Setter(Class1.FooProperty, "style") },
            };

            target.Bind(Class1.FooProperty, source, BindingPriority.Animation);
            ((IStyleable)target).ApplyStyle(style);

            Assert.Equal(1, target.CoreChanges.Count);
            Assert.Equal(1, target.Changes.Count);

            target.Classes.Add("foo");

            Assert.Equal(2, target.CoreChanges.Count);
            Assert.Equal(1, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[1];

            Assert.Equal("style", change.NewValue.Value);
            Assert.Equal("foodefault", change.OldValue.Value);
            Assert.Equal(BindingPriority.StyleTrigger, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Setter_Binding_Change()
        {
            var target = new Class1();
            var source = new BehaviorSubject<BindingValue<string>>("animation");
            var binding = new TestStyleBinding("foo");
            var style = new Style(x => x.OfType<Class1>())
            {
                Setters = { new Setter(Class1.FooProperty, binding) },
            };

            ((IStyleable)target).ApplyStyle(style);

            // Set the animation after applying the style to be sure the binding is started.
            target.Bind(Class1.FooProperty, source, BindingPriority.Animation);

            Assert.Equal(2, target.CoreChanges.Count);
            Assert.Equal(2, target.Changes.Count);

            binding.OnNext("bar");

            Assert.Equal(3, target.CoreChanges.Count);
            Assert.Equal(2, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[2];

            Assert.Equal("bar", change.NewValue.Value);
            Assert.Equal("foodefault", change.OldValue.Value);
            Assert.Equal(BindingPriority.Style, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        [Fact]
        public void OnPropertyChangedCore_Is_Called_On_Non_Effective_Style_Binding_Change()
        {
            var target = new Class1();
            var source1 = new BehaviorSubject<BindingValue<string>>("animation");
            var source2 = new BehaviorSubject<BindingValue<string>>("style");

            target.Bind(Class1.FooProperty, source1, BindingPriority.Animation);
            target.Bind(Class1.FooProperty, source2, BindingPriority.Style);

            Assert.Equal(2, target.CoreChanges.Count);
            Assert.Equal(1, target.Changes.Count);

            var change = (AvaloniaPropertyChangedEventArgs<string>)target.CoreChanges[1];

            Assert.Equal("style", change.NewValue.Value);
            Assert.False(change.OldValue.HasValue);
            Assert.Equal(BindingPriority.Style, change.Priority);
            Assert.False(change.IsEffectiveValueChange);
        }

        private class Class1 : Control
        {
            public static readonly StyledProperty<string> FooProperty =
                AvaloniaProperty.Register<Class1, string>("Foo", "foodefault");

            public Class1()
            {
                Changes = new List<AvaloniaPropertyChangedEventArgs>();
                CoreChanges = new List<AvaloniaPropertyChangedEventArgs>();
            }

            public List<AvaloniaPropertyChangedEventArgs> Changes { get; }
            public List<AvaloniaPropertyChangedEventArgs> CoreChanges { get; }

            private protected override void OnPropertyChangedCore<T>(AvaloniaPropertyChangedEventArgs<T> change)
            {
                CoreChanges.Add(Clone(change));
                base.OnPropertyChangedCore(change);
            }

            protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
            {
                Changes.Add(Clone(change));
                base.OnPropertyChanged(change);
            }

            private static AvaloniaPropertyChangedEventArgs<T> Clone<T>(AvaloniaPropertyChangedEventArgs<T> change)
            {
                return new AvaloniaPropertyChangedEventArgs<T>(
                    change.Sender,
                    change.Property,
                    change.OldValue,
                    change.NewValue,
                    change.Priority,
                    change.IsEffectiveValueChange);
            }
        }
    }
}
