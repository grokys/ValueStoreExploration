using System;
using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_Coercion
    {
        [Fact]
        public void Coerces_Set_Value()
        {
            var target = new Class1();

            target.Foo = 150;

            Assert.Equal(100, target.Foo);
        }

        [Fact]
        public void Coerces_Set_Value_Attached()
        {
            var target = new Class1();

            target.SetValue(Class1.AttachedProperty, 150);

            Assert.Equal(100, target.GetValue(Class1.AttachedProperty));
        }

        [Fact]
        public void Coerces_Bound_Value()
        {
            var target = new Class1();
            var source = new Subject<BindingValue<int>>();

            target.Bind(Class1.FooProperty, source);
            source.OnNext(150);

            Assert.Equal(100, target.Foo);
        }

        [Fact]
        public void Coerces_Style_Value()
        {
            var target = new Class1();
            var style = new Style(x => x.OfType<Class1>())
            {
                Setters = { new Setter(Class1.FooProperty, 150 )},
            };

            ((IStyleable)target).ApplyStyle(style);

            Assert.Equal(100, target.Foo);
        }

        [Fact]
        public void Coerces_Triggered_Style_Value()
        {
            var target = new Class1();
            var style = new Style(x => x.OfType<Class1>().Class("foo"))
            {
                Setters = { new Setter(Class1.FooProperty, 150) },
            };

            ((IStyleable)target).ApplyStyle(style);
            
            Assert.Equal(11, target.Foo);

            target.Classes.Add("foo");

            Assert.Equal(100, target.Foo);
        }

        [Fact]
        public void CoerceValue_Updates_Value()
        {
            var target = new Class1 { Foo = 99 };

            Assert.Equal(99, target.Foo);

            target.MaxFoo = 50;
            target.CoerceValue(Class1.FooProperty);

            Assert.Equal(50, target.Foo);
        }

        [Fact]
        public void Coerced_Value_Can_Be_Restored_If_Limit_Changed()
        {
            var target = new Class1();

            target.Foo = 150;
            Assert.Equal(100, target.Foo);

            target.MaxFoo = 200;
            target.CoerceValue(Class1.FooProperty);

            Assert.Equal(150, target.Foo);
        }

        [Fact]
        public void Coerced_Value_Can_Be_Restored_From_Previously_Active_Binding()
        {
            var target = new Class1();
            var source1 = new Subject<BindingValue<int>>();
            var source2 = new Subject<BindingValue<int>>();

            target.Bind(Class1.FooProperty, source1);
            source1.OnNext(150);

            target.Bind(Class1.FooProperty, source2);
            source2.OnNext(160);

            Assert.Equal(100, target.Foo);

            target.MaxFoo = 200;
            source2.OnCompleted();

            Assert.Equal(150, target.Foo);
        }

        [Fact]
        public void Coercion_Can_Be_Overridden()
        {
            var target = new Class2();

            target.Foo = 150;

            Assert.Equal(-150, target.Foo);
        }

        [Fact]
        public void Coerced_Value_Is_Reported_In_PropertyChanged()
        {
            var target = new Class1();
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.Property == Class1.FooProperty)
                {
                    Assert.Equal(100, (int)e.NewValue!);
                    ++raised;
                }
            };

            target.Foo = 150;

            Assert.Equal(1, raised);
        }

        [Fact]
        public void Coerced_Value_Is_Reported_In_Binding_PropertyChanged()
        {
            var target = new Class1();
            var source = new BehaviorSubject<BindingValue<int>>(150);
            var raised = 0;

            target.PropertyChanged += (s, e) =>
            {
                if (e.Property == Class1.FooProperty)
                {
                    Assert.Equal(100, (int)e.NewValue!);
                    ++raised;
                }
            };

            target.Bind(Class1.FooProperty, source);

            Assert.Equal(1, raised);
        }

        private class Class1 : Control
        {
            public static readonly StyledProperty<int> FooProperty =
                AvaloniaProperty.Register<Class1, int>(
                    "Qux",
                    defaultValue: 11,
                    coerce: CoerceFoo);

            public static readonly AttachedProperty<int> AttachedProperty =
                AvaloniaProperty.RegisterAttached<Class1, Class1, int>(
                    "Attached",
                    defaultValue: 11,
                    coerce: CoerceFoo);

            public int Foo
            {
                get => GetValue(FooProperty);
                set => SetValue(FooProperty, value);
            }

            public int MaxFoo { get; set; } = 100;

            public static int CoerceFoo(IAvaloniaObject instance, int value)
            {
                return Math.Min(((Class1)instance).MaxFoo, value);
            }
        }

        private class Class2 : AvaloniaObject
        {
            public static readonly StyledProperty<int> FooProperty =
                Class1.FooProperty.AddOwner<Class2>();

            static Class2()
            {
                FooProperty.OverrideMetadata<Class2>(
                    new StyledPropertyMetadata<int>(
                        coerce: CoerceFoo));
            }

            public int Foo
            {
                get => GetValue(FooProperty);
                set => SetValue(FooProperty, value);
            }

            public static int CoerceFoo(IAvaloniaObject instance, int value)
            {
                return -value;
            }
        }
    }
}
