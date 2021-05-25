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

#if !BOXING
        [Fact]
        public void Applies_Style_With_Typed_Setter()
        {
            var target = new Class1();
            var style = new Style(x => x.OfType<Class1>())
            {
                Setters = { new Setter<string?>(Class1.FooProperty, "foo") }
            };

            ApplyStyles(target, style);

            Assert.Equal("foo", target.Foo);
        }
#endif

        private void ApplyStyles(IStyleable target, params Style[] styles)
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
