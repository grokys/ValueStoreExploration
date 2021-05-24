using BenchmarkDotNet.Attributes;

namespace Avalonia.Benchmarks
{
    [MemoryDiagnoser]
    public class SetGetValueBenchmarks
    {
        [Benchmark]
        public void SetValues()
        {
            var target = new TestClass();

            for (var i = 0; i < 100; ++i)
            {
                target.SetValue(TestClass.StringProperty, "foo");
                target.SetValue(TestClass.Struct1Property, new Struct1());
                target.SetValue(TestClass.Struct2Property, new Struct2());
                target.SetValue(TestClass.Struct3Property, new Struct3());
                target.SetValue(TestClass.Struct4Property, new Struct4());
                target.SetValue(TestClass.Struct5Property, new Struct5());
                target.SetValue(TestClass.Struct6Property, new Struct6());
                target.SetValue(TestClass.Struct7Property, new Struct7());
                target.SetValue(TestClass.Struct8Property, new Struct8());
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

        [Benchmark]
        public void GetValues()
        {
            var target = new TestClass();

            target.SetValue(TestClass.StringProperty, "foo");
            target.SetValue(TestClass.Struct1Property, new Struct1());
            target.SetValue(TestClass.Struct2Property, new Struct2());
            target.SetValue(TestClass.Struct3Property, new Struct3());
            target.SetValue(TestClass.Struct4Property, new Struct4());
            target.SetValue(TestClass.Struct5Property, new Struct5());
            target.SetValue(TestClass.Struct6Property, new Struct6());
            target.SetValue(TestClass.Struct7Property, new Struct7());
            target.SetValue(TestClass.Struct8Property, new Struct8());

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

        private struct Struct1 : System.IEquatable<Struct1>
        {
            public int Int1;

            public override bool Equals(object? obj)
            {
                return obj is Struct1 @struct && Equals(@struct);
            }

            public bool Equals(Struct1 other)
            {
                return Int1 == other.Int1;
            }
        }

        private struct Struct2 : System.IEquatable<Struct2>
        {
            public int Int1;
            public int Int2;

            public override bool Equals(object? obj)
            {
                return obj is Struct2 @struct && Equals(@struct);
            }

            public bool Equals(Struct2 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2;
            }
        }

        private struct Struct3 : System.IEquatable<Struct3>
        {
            public int Int1;
            public int Int2;
            public int Int3;

            public override bool Equals(object? obj)
            {
                return obj is Struct3 @struct && Equals(@struct);
            }

            public bool Equals(Struct3 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3;
            }
        }

        private struct Struct4 : System.IEquatable<Struct4>
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;

            public override bool Equals(object? obj)
            {
                return obj is Struct4 @struct && Equals(@struct);
            }

            public bool Equals(Struct4 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3 &&
                       Int4 == other.Int4;
            }
        }

        private struct Struct5 : System.IEquatable<Struct5>
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;
            public int Int5;

            public override bool Equals(object? obj)
            {
                return obj is Struct5 @struct && Equals(@struct);
            }

            public bool Equals(Struct5 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3 &&
                       Int4 == other.Int4 &&
                       Int5 == other.Int5;
            }
        }

        private struct Struct6 : System.IEquatable<Struct6>
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;
            public int Int5;
            public int Int6;

            public override bool Equals(object? obj)
            {
                return obj is Struct6 @struct && Equals(@struct);
            }

            public bool Equals(Struct6 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3 &&
                       Int4 == other.Int4 &&
                       Int5 == other.Int5 &&
                       Int6 == other.Int6;
            }
        }

        private struct Struct7 : System.IEquatable<Struct7>
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;
            public int Int5;
            public int Int6;
            public int Int7;

            public override bool Equals(object? obj)
            {
                return obj is Struct7 @struct && Equals(@struct);
            }

            public bool Equals(Struct7 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3 &&
                       Int4 == other.Int4 &&
                       Int5 == other.Int5 &&
                       Int6 == other.Int6 &&
                       Int7 == other.Int7;
            }
        }

        private struct Struct8 : System.IEquatable<Struct8>
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;
            public int Int5;
            public int Int6;
            public int Int7;

            public override bool Equals(object? obj)
            {
                return obj is Struct8 @struct && Equals(@struct);
            }

            public bool Equals(Struct8 other)
            {
                return Int1 == other.Int1 &&
                       Int2 == other.Int2 &&
                       Int3 == other.Int3 &&
                       Int4 == other.Int4 &&
                       Int5 == other.Int5 &&
                       Int6 == other.Int6 &&
                       Int7 == other.Int7;
            }
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
