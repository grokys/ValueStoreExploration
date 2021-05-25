using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_Styling
    {
        [Fact]
        public void Applies_Style_With_Untyped_Setter()
        {
            var target = new Class1();
            var style = new Style(x => x.OfType<Class1>())
            {
                Setters = { new Setter(Class1.FooProperty, "foo") }
            };

            ApplyStyles(target, style);

            Assert.Equal("foo", target.Foo);
        }

        [Fact]
        public void Later_Style_Overrides_Prior()
        {
            var target = new Class1();
            var styles = new[]
            {
                new Style(x => x.OfType<Class1>())
                {
                    Setters = { new Setter(Class1.FooProperty, "foo") }
                },
                new Style(x => x.OfType<Class1>())
                {
                    Setters = { new Setter(Class1.FooProperty, "bar") }
                },
            };

            ApplyStyles(target, styles);

            Assert.Equal("bar", target.Foo);
        }

        [Fact]
        public void Later_Style_Doesnt_Override_Prior_Style_Of_Higher_Priority()
        {
            var target = new Class1 { Classes = { "class" } };
            var styles = new[]
            {
                new Style(x => x.OfType<Class1>().Class("class"))
                {
                    Setters = { new Setter(Class1.FooProperty, "foo") }
                },
                new Style(x => x.OfType<Class1>())
                {
                    Setters = { new Setter(Class1.FooProperty, "bar") }
                },
            };

            ApplyStyles(target, styles);

            Assert.Equal("foo", target.Foo);
        }

        [Fact]
        public void Style_With_Class_Selector_Should_Update_And_Restore_Value()
        {
            var target = new Class1();
            var style = new Style(x => x.OfType<Class1>().Class("foo"))
            {
                Setters = { new Setter(Class1.FooProperty, "Foo") },
            };

            ApplyStyles(target, style);

            Assert.Equal("foodefault", target.Foo);
            target.Classes.Add("foo");
            Assert.Equal("Foo", target.Foo);
            target.Classes.Remove("foo");
            Assert.Equal("foodefault", target.Foo);
        }

        [Fact]
        public void Raises_PropertyChanged_For_Style_Activation_Changes_1()
        {
            var styles = new[]
            {
                new Style(x => x.OfType<Class1>().Class("foo"))
                {
                    Setters = { new Setter(Class1.FooProperty, "Foo") },
                },

                new Style(x => x.OfType<Class1>().Class("bar"))
                {
                    Setters = { new Setter(Class1.FooProperty, "Bar") },
                }
            };

            var target = new Class1();
            var values = new List<string?> { target.Foo };

            target.PropertyChanged += (s, e) =>
            {
                if (e.Property == Class1.FooProperty)
                {
                    values.Add((string?)e.NewValue);
                }
            };

            ApplyStyles(target, styles);
            target.Classes.Add("foo");
            target.Classes.Add("bar");
            target.Classes.Remove("foo");
            target.Classes.Remove("bar");

            Assert.Equal(new[] { "foodefault", "Foo", "Bar", "foodefault" }, values);
        }

        private static void ApplyStyles(IStyleable target, params Style[] styles)
        {
            target.BeginStyling();
            foreach (var style in styles)
                target.ApplyStyle(style);
            target.EndStyling();
        }

        private class Class1 : Control
        {
            public static readonly StyledProperty<string?> FooProperty = 
                AvaloniaProperty.Register<Class1, string?>(nameof(Foo), "foodefault");

            public string? Foo
            {
                get => GetValue(FooProperty);
                set => SetValue(FooProperty, value);
            }
        }
    }
}
