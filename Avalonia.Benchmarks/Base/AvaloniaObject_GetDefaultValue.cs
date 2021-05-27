using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Avalonia.Benchmarks.Base
{
    [MemoryDiagnoser]
    public class AvaloniaObject_GetDefaultValue
    {
        private TestClass _target = null!;

        public AvaloniaObject_GetDefaultValue()
        {
            RuntimeHelpers.RunClassConstructor(typeof(TestClass).TypeHandle);
        }

        [GlobalSetup]
        public void Setup()
        {
            _target = new TestClass();
        }

        [Benchmark]
        public int GetDefaultValue()
        {
            var target = _target;
            var result = 0;

            for (var i = 0; i < 100; ++i)
            {
                result += target.GetValue(TestClass.StringProperty)?.Length ?? 0;
                result += target.GetValue(TestClass.Struct1Property).Int1;
                result += target.GetValue(TestClass.Struct2Property).Int1;
                result += target.GetValue(TestClass.Struct3Property).Int1;
                result += target.GetValue(TestClass.Struct4Property).Int1;
                result += target.GetValue(TestClass.Struct5Property).Int1;
                result += target.GetValue(TestClass.Struct6Property).Int1;
                result += target.GetValue(TestClass.Struct7Property).Int1;
                result += target.GetValue(TestClass.Struct8Property).Int1;
            }

            return result;
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
