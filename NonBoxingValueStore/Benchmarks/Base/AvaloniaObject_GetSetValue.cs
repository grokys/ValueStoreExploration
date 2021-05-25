using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Avalonia.Benchmarks.Base
{
    [MemoryDiagnoser]
    public class AvaloniaObject_GetSetValue
    {
        private TestClass _target = null!;

        public AvaloniaObject_GetSetValue()
        {
            RuntimeHelpers.RunClassConstructor(typeof(TestClass).TypeHandle);
        }

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestClass();
        }

        [Benchmark]
        public void GetDefaultValues()
        {
            var target = _target;

            for (var i = 0; i < 100; ++i)
            {
                var v0 = target.GetValue(TestClass.StringProperty);
                var v1 = target.GetValue(TestClass.Struct1Property);
                var v2 = target.GetValue(TestClass.Struct2Property);
                var v3 = target.GetValue(TestClass.Struct3Property);
                var v4 = target.GetValue(TestClass.Struct4Property);
                var v5 = target.GetValue(TestClass.Struct5Property);
                var v6 = target.GetValue(TestClass.Struct6Property);
                var v7 = target.GetValue(TestClass.Struct7Property);
                var v8 = target.GetValue(TestClass.Struct8Property);
            }
        }

        [Benchmark]
        public void GetValues()
        {
            var target = _target;

            target.SetValue(TestClass.StringProperty, "foo");
            target.SetValue(TestClass.Struct1Property, new(1));
            target.SetValue(TestClass.Struct2Property, new(1));
            target.SetValue(TestClass.Struct3Property, new(1));
            target.SetValue(TestClass.Struct4Property, new(1));
            target.SetValue(TestClass.Struct5Property, new(1));
            target.SetValue(TestClass.Struct6Property, new(1));
            target.SetValue(TestClass.Struct7Property, new(1));
            target.SetValue(TestClass.Struct8Property, new(1));

            for (var i = 0; i < 100; ++i)
            {
                var v0 = target.GetValue(TestClass.StringProperty);
                var v1 = target.GetValue(TestClass.Struct1Property);
                var v2 = target.GetValue(TestClass.Struct2Property);
                var v3 = target.GetValue(TestClass.Struct3Property);
                var v4 = target.GetValue(TestClass.Struct4Property);
                var v5 = target.GetValue(TestClass.Struct5Property);
                var v6 = target.GetValue(TestClass.Struct6Property);
                var v7 = target.GetValue(TestClass.Struct7Property);
                var v8 = target.GetValue(TestClass.Struct8Property);
            }
        }

        [Benchmark]
        public void SetValues()
        {
            var target = _target;

            for (var i = 0; i < 100; ++i)
            {
                target.SetValue(TestClass.StringProperty, "foo");
                target.SetValue(TestClass.Struct1Property, new(i + 1));
                target.SetValue(TestClass.Struct2Property, new(i + 1));
                target.SetValue(TestClass.Struct3Property, new(i + 1));
                target.SetValue(TestClass.Struct4Property, new(i + 1));
                target.SetValue(TestClass.Struct5Property, new(i + 1));
                target.SetValue(TestClass.Struct6Property, new(i + 1));
                target.SetValue(TestClass.Struct7Property, new(i + 1));
                target.SetValue(TestClass.Struct8Property, new(i + 1));
            }
            
            var v0 = target.GetValue(TestClass.StringProperty);
            var v1 = target.GetValue(TestClass.Struct1Property);
            var v2 = target.GetValue(TestClass.Struct2Property);
            var v3 = target.GetValue(TestClass.Struct3Property);
            var v4 = target.GetValue(TestClass.Struct4Property);
            var v5 = target.GetValue(TestClass.Struct5Property);
            var v6 = target.GetValue(TestClass.Struct6Property);
            var v7 = target.GetValue(TestClass.Struct7Property);
            var v8 = target.GetValue(TestClass.Struct8Property);
        }

        private class TestClass : AvaloniaObject
        {
            public static readonly StyledProperty<string> StringProperty =
                AvaloniaProperty.Register<TestClass, string>("String");
            public static readonly StyledProperty<Struct1> Struct1Property =
                AvaloniaProperty.Register<TestClass, Struct1>("Struct1");
            public static readonly StyledProperty<Struct2> Struct2Property =
                AvaloniaProperty.Register<TestClass, Struct2>("Struct2");
            public static readonly StyledProperty<Struct3> Struct3Property =
                AvaloniaProperty.Register<TestClass, Struct3>("Struct3");
            public static readonly StyledProperty<Struct4> Struct4Property =
                AvaloniaProperty.Register<TestClass, Struct4>("Struct4");
            public static readonly StyledProperty<Struct5> Struct5Property =
                AvaloniaProperty.Register<TestClass, Struct5>("Struct5");
            public static readonly StyledProperty<Struct6> Struct6Property =
                AvaloniaProperty.Register<TestClass, Struct6>("Struct6");
            public static readonly StyledProperty<Struct7> Struct7Property =
                AvaloniaProperty.Register<TestClass, Struct7>("Struct7");
            public static readonly StyledProperty<Struct8> Struct8Property =
                AvaloniaProperty.Register<TestClass, Struct8>("Struct8");
        }
    }
}
