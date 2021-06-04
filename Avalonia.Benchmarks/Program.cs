using System;
using Avalonia.Benchmarks.Styling;
using BenchmarkDotNet.Running;

namespace Avalonia
{
    class Program
    {
        public static void Main(string[] args)
        {
            var b = new Style_NonActive();
            b.Setup();
            Console.WriteLine("Start"); Console.ReadKey();
            b.Toggle_NonActive_Style_Activation();
            Console.WriteLine("End"); Console.ReadKey();
            ///BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
