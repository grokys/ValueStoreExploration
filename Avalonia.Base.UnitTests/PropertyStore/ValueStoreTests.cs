using System.Reactive.Subjects;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Xunit;

namespace Avalonia.Tests.PropertyStore
{
    public class ValueStoreTests
    {
        [Fact]
        public void Frames_Are_Initially_Empty()
        {
            var (_, target) = CreateTarget();

            Assert.Empty(target.Frames);
        }

        [Fact]
        public void Setting_Value_Adds_LocalValueFrame_And_Entry()
        {
            var (o, target) = CreateTarget();

            o.SetValue(Class1.FooProperty, "foo");

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);
            var entry = Assert.IsType<LocalValueEntry<string>>(frame.Values[0]);
        }

        [Fact]
        public void ClearValue_Removes_LocalValueEntry()
        {
            var (o, target) = CreateTarget();

            o.SetValue(Class1.FooProperty, "foo");

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);

            o.ClearValue(Class1.FooProperty);

            Assert.Equal(0, frame.Values.Count);
        }

        [Fact]
        public void Adding_LocalValue_Binding_Adds_LocalValueFrame_And_Entry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");

            o.Bind(Class1.FooProperty, source);

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);
            var entry = Assert.IsType<LocalValueEntry<string>>(frame.Values[0]);
        }

        [Fact]
        public void Adding_Style_Binding_Adds_BindingEntry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");

            o.Bind(Class1.FooProperty, source, BindingPriority.Style);

            Assert.Equal(1, target.Frames.Count);
            Assert.IsType<BindingEntry<string?>>(target.Frames[0]);
        }

        [Fact]
        public void Completing_LocalValue_Binding_Removes_LocalValueEntry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");
            var sub = o.Bind(Class1.FooProperty, source);

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);

            sub.Dispose();

            Assert.Equal(0, frame.Values.Count);
        }

        [Fact]
        public void Disposing_LocalValue_Binding_Removes_LocalValueEntry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");
            var sub = o.Bind(Class1.FooProperty, source);

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);

            sub.Dispose();

            Assert.Equal(0, frame.Values.Count);
        }

        [Fact]
        public void Completing_Style_Binding_Removes_BindingEntry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");
            var sub = o.Bind(Class1.FooProperty, source, BindingPriority.Style);

            Assert.Equal(1, target.Frames.Count);

            sub.Dispose();

            Assert.Equal(0, target.Frames.Count);
        }

        [Fact]
        public void Disposing_Style_Binding_Removes_BindingEntry()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");
            var sub = o.Bind(Class1.FooProperty, source, BindingPriority.Style);

            Assert.Equal(1, target.Frames.Count);

            sub.Dispose();

            Assert.Equal(0, target.Frames.Count);
        }

        [Fact]
        public void ClearValue_Doesnt_Remove_LocalValueEntry_If_Binding_Active()
        {
            var (o, target) = CreateTarget();
            var source = new BehaviorSubject<BindingValue<string>>("foo");

            o.Bind(Class1.FooProperty, source);

            Assert.Equal(1, target.Frames.Count);
            var frame = Assert.IsType<LocalValueFrame>(target.Frames[0]);
            Assert.Equal(1, frame.Values.Count);

            o.ClearValue(Class1.FooProperty);

            Assert.Equal(1, frame.Values.Count);
        }

        private static (AvaloniaObject, ValueStore) CreateTarget()
        {
            var o = new AvaloniaObject();
            return (o, o.UnitTestGetValueStore());
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly StyledProperty<string> FooProperty =
                AvaloniaProperty.Register<Class1, string>("Foo", "foodefault");
        }
    }
}
