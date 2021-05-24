using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Avalonia.Data;
using BenchmarkDotNet.Attributes;

namespace Avalonia.Benchmarks.AvaloniaObjectBenchmarks
{
    [MemoryDiagnoser]
    public class BindingBenchmarks
    {
        private static TestBindingObservable<string?> s_stringSource = null!;
        private static TestBindingObservable<Struct1> s_struct1Source = null!;
        private static TestBindingObservable<Struct2> s_struct2Source = null!;
        private static TestBindingObservable<Struct3> s_struct3Source = null!;
        private static TestBindingObservable<Struct4> s_struct4Source = null!;
        private static TestBindingObservable<Struct5> s_struct5Source = null!;
        private static TestBindingObservable<Struct6> s_struct6Source = null!;
        private static TestBindingObservable<Struct7> s_struct7Source = null!;
        private static TestBindingObservable<Struct8> s_struct8Source = null!;

        [GlobalSetup]
        public void Setup()
        {
            RuntimeHelpers.RunClassConstructor(typeof(TestClass).TypeHandle);
            s_stringSource = new();
            s_struct1Source = new();
            s_struct2Source = new();
            s_struct3Source = new();
            s_struct4Source = new();
            s_struct5Source = new();
            s_struct6Source = new();
            s_struct7Source = new();
            s_struct8Source = new();
        }

        [Benchmark]
        public void Setup_Dispose_LocalValue_Bindings()
        {
            var target = new TestClass();

            for (var i = 0; i < 100; ++i)
            {
                using var s0 = target.Bind(TestClass.StringProperty, s_stringSource);
                using var s1 = target.Bind(TestClass.Struct1Property, s_struct1Source);
                using var s2 = target.Bind(TestClass.Struct2Property, s_struct2Source);
                using var s3 = target.Bind(TestClass.Struct3Property, s_struct3Source);
                using var s4 = target.Bind(TestClass.Struct4Property, s_struct4Source);
                using var s5 = target.Bind(TestClass.Struct5Property, s_struct5Source);
                using var s6 = target.Bind(TestClass.Struct6Property, s_struct6Source);
                using var s7 = target.Bind(TestClass.Struct7Property, s_struct7Source);
                using var s8 = target.Bind(TestClass.Struct8Property, s_struct8Source);
            }
        }


        [Benchmark]
        public void Fire_LocalValue_Bindings()
        {
            var target = new TestClass();

            using var s0 = target.Bind(TestClass.StringProperty, s_stringSource);
            using var s1 = target.Bind(TestClass.Struct1Property, s_struct1Source);
            using var s2 = target.Bind(TestClass.Struct2Property, s_struct2Source);
            using var s3 = target.Bind(TestClass.Struct3Property, s_struct3Source);
            using var s4 = target.Bind(TestClass.Struct4Property, s_struct4Source);
            using var s5 = target.Bind(TestClass.Struct5Property, s_struct5Source);
            using var s6 = target.Bind(TestClass.Struct6Property, s_struct6Source);
            using var s7 = target.Bind(TestClass.Struct7Property, s_struct7Source);
            using var s8 = target.Bind(TestClass.Struct8Property, s_struct8Source);

            for (var i = 0; i < 100; ++i)
            {
                s_stringSource.OnNext(i.ToString());
                s_struct1Source.OnNext(new(i + 1));
                s_struct2Source.OnNext(new(i + 1));
                s_struct3Source.OnNext(new(i + 1));
                s_struct4Source.OnNext(new(i + 1));
                s_struct5Source.OnNext(new(i + 1));
                s_struct6Source.OnNext(new(i + 1));
                s_struct7Source.OnNext(new(i + 1));
                s_struct8Source.OnNext(new(i + 1));
            }
        }

        private class TestClass : AvaloniaObject
        {
            public static readonly StyledProperty<string?> StringProperty =
                AvaloniaProperty.Register<TestClass, string?>("String");
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
