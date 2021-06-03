# Experiments with the Avalonia value store

Trying out a new way of storing values to increase performance of the Avalonia value store.

A probably incomplete todo:

- [x] Non-local value bindings
- [x] Animation bindings
- [ ] Template support in setters
- [x] Property value inhertance
- [x] Validation
- [ ] Coercion
- [x] GetBaseValue - renamed to `GetValueByPriority` (https://github.com/AvaloniaUI/Avalonia/pull/3853#discussion_r426188762)
- [x] GetObservable
- [ ] OnPropertyChangedCore

# Benchmarks

## Avalonia `master`

|           Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |---------:|---------:|---------:|------:|------:|------:|----------:|
| GetDefaultValues | 17.44 us | 0.077 us | 0.068 us |     - |     - |     - |         - |

|               Method |        Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |------------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
| GetClrPropertyValues |    449.9 ns |   1.38 ns |   1.29 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|            GetValues | 40,669.5 ns | 149.28 ns | 132.34 ns | 90.36 |    0.40 |     - |     - |     - |         - |

|             Method | Depth |        Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |------ |------------:|---------:|---------:|------:|------:|------:|----------:|
| GetInheritedValues |     1 |    55.43 us | 0.418 us | 0.349 us |     - |     - |     - |         - |
| GetInheritedValues |     2 |    59.66 us | 0.207 us | 0.183 us |     - |     - |     - |         - |
| GetInheritedValues |    10 |   102.71 us | 0.297 us | 0.248 us |     - |     - |     - |         - |
| GetInheritedValues |    50 |   282.54 us | 0.957 us | 0.848 us |     - |     - |     - |       1 B |
| GetInheritedValues |   100 |   509.31 us | 3.806 us | 3.560 us |     - |     - |     - |       5 B |
| GetInheritedValues |   200 | 1,005.10 us | 0.846 us | 0.661 us |     - |     - |     - |       3 B |

|               Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|----------:|----------:|------:|--------:|--------:|------:|------:|----------:|
| SetClrPropertyValues |   5.762 us | 0.0346 us | 0.0307 us |  1.00 |    0.00 |       - |     - |     - |         - |
|            SetValues | 227.889 us | 1.9449 us | 1.7241 us | 39.55 |    0.37 | 20.9961 |     - |     - |  88,000 B |

|                            Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
| Toggle_Style_Activation_Via_Class | 106.6 us | 0.37 us | 0.33 us | 8.7891 |     - |     - |     36 KB |

|                            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
| Toggle_NonActive_Style_Activation | 96.10 us | 0.602 us | 0.534 us | 8.7891 |     - |     - |     36 KB |

|                            Method |     Mean |   Error |  StdDev |    Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|---------:|------:|------:|----------:|
| Setup_Dispose_LocalValue_Bindings | 454.8 us | 2.17 us | 2.03 us | 107.9102 |     - |     - |    441 KB |
|          Fire_LocalValue_Bindings | 175.3 us | 1.34 us | 1.19 us |  22.7051 |     - |     - |     93 KB |

|              Method | MatchingStyles | NonMatchingStyles |      Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |--------------- |------------------ |----------:|----------:|----------:|-------:|------:|------:|----------:|
| Apply_Simple_Styles |              1 |                 1 |  1.229 us | 0.0064 us | 0.0050 us | 0.3815 |     - |     - |      2 KB |
| Apply_Simple_Styles |              1 |                 5 |  1.411 us | 0.0074 us | 0.0066 us | 0.3815 |     - |     - |      2 KB |
| Apply_Simple_Styles |              1 |                50 |  3.215 us | 0.0425 us | 0.0398 us | 0.3815 |     - |     - |      2 KB |
| Apply_Simple_Styles |              5 |                 1 |  4.329 us | 0.0817 us | 0.0803 us | 0.8240 |     - |     - |      3 KB |
| Apply_Simple_Styles |              5 |                 5 |  4.443 us | 0.0878 us | 0.0862 us | 0.8240 |     - |     - |      3 KB |
| Apply_Simple_Styles |              5 |                50 |  5.805 us | 0.1106 us | 0.0924 us | 0.8240 |     - |     - |      3 KB |
| Apply_Simple_Styles |             50 |                 1 | 32.370 us | 0.1779 us | 0.1664 us | 5.3101 |     - |     - |     22 KB |
| Apply_Simple_Styles |             50 |                 5 | 31.446 us | 0.1676 us | 0.1485 us | 5.3101 |     - |     - |     22 KB |
| Apply_Simple_Styles |             50 |                50 | 33.883 us | 0.5897 us | 0.6310 us | 5.3101 |     - |     - |     22 KB |

|                      Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |---------:|--------:|--------:|------:|--------:|--------:|------:|------:|----------:|
| PropertyChangedSubscription | 349.9 us | 6.80 us | 9.07 us |  1.00 |    0.00 | 22.4609 |     - |     - |     92 KB |
|              GetObservables | 565.6 us | 2.21 us | 2.07 us |  1.63 |    0.04 | 23.4375 |     - |     - |     97 KB |

## New Value Store (this repository)

At 01c6e1f049d428f48dbdfbc83b82976285d5ca5f

|           Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |---------:|----------:|----------:|------:|------:|------:|----------:|
| GetDefaultValues | 5.720 us | 0.0407 us | 0.0361 us |     - |     - |     - |         - |

|               Method |        Mean |     Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |------------:|----------:|---------:|------:|--------:|------:|------:|------:|----------:|
| GetClrPropertyValues |    436.6 ns |   1.28 ns |  1.07 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|            GetValues | 15,197.6 ns | 100.19 ns | 93.72 ns | 34.79 |    0.25 |     - |     - |     - |         - |

|             Method | Depth |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |------ |---------:|---------:|---------:|------:|------:|------:|----------:|
| GetInheritedValues |     1 | 20.29 us | 0.131 us | 0.123 us |     - |     - |     - |         - |
| GetInheritedValues |     2 | 19.79 us | 0.128 us | 0.100 us |     - |     - |     - |         - |
| GetInheritedValues |    10 | 19.85 us | 0.126 us | 0.118 us |     - |     - |     - |         - |
| GetInheritedValues |    50 | 21.07 us | 0.412 us | 0.754 us |     - |     - |     - |         - |
| GetInheritedValues |   100 | 19.87 us | 0.121 us | 0.113 us |     - |     - |     - |         - |
| GetInheritedValues |   200 | 19.67 us | 0.109 us | 0.085 us |     - |     - |     - |         - |

|               Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
| SetClrPropertyValues |   5.733 us | 0.0495 us | 0.0463 us |  1.00 |    0.00 |     - |     - |     - |         - |
|            SetValues | 231.904 us | 1.1388 us | 1.0653 us | 40.45 |    0.29 |     - |     - |     - |         - |

|                            Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
| Setup_Dispose_LocalValue_Bindings | 284.0 us | 5.36 us | 5.26 us | 0.4883 |     - |     - |      2 KB |
|          Fire_LocalValue_Bindings | 222.0 us | 0.36 us | 0.30 us | 1.2207 |     - |     - |      5 KB |

|                            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
| Toggle_Style_Activation_Via_Class | 70.38 us | 0.155 us | 0.137 us | 4.8828 |     - |     - |     20 KB |

|                            Method |     Mean |   Error |  StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|--------:|------:|------:|----------:|
| Toggle_NonActive_Style_Activation | 312.3 us | 1.57 us | 1.47 us | 32.2266 |     - |     - |    133 KB |

|              Method | MatchingStyles | NonMatchingStyles |       Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |--------------- |------------------ |-----------:|----------:|----------:|-------:|------:|------:|----------:|
| Apply_Simple_Styles |              1 |                 1 |   532.8 ns |   1.96 ns |   1.64 ns | 0.0973 |     - |     - |     408 B |
| Apply_Simple_Styles |              1 |                 5 |   645.2 ns |   2.43 ns |   2.03 ns | 0.0973 |     - |     - |     408 B |
| Apply_Simple_Styles |              1 |                50 | 1,876.2 ns |  15.13 ns |  12.64 ns | 0.0973 |     - |     - |     408 B |
| Apply_Simple_Styles |              5 |                 1 | 1,040.2 ns |   4.56 ns |   4.04 ns | 0.1183 |     - |     - |     496 B |
| Apply_Simple_Styles |              5 |                 5 | 1,248.5 ns |  24.97 ns |  38.13 ns | 0.1183 |     - |     - |     496 B |
| Apply_Simple_Styles |              5 |                50 | 2,708.5 ns |  33.04 ns |  27.59 ns | 0.1183 |     - |     - |     496 B |
| Apply_Simple_Styles |             50 |                 1 | 7,912.6 ns | 139.02 ns | 130.04 ns | 0.3357 |     - |     - |   1,464 B |
| Apply_Simple_Styles |             50 |                 5 | 7,418.9 ns |  98.30 ns | 140.98 ns | 0.3357 |     - |     - |   1,464 B |
| Apply_Simple_Styles |             50 |                50 | 8,480.1 ns |  41.36 ns |  38.68 ns | 0.3357 |     - |     - |   1,464 B |

|                      Method |     Mean |   Error |  StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |---------:|--------:|--------:|------:|-------:|------:|------:|----------:|
| PropertyChangedSubscription | 359.5 us | 1.56 us | 1.46 us |  1.00 | 0.9766 |     - |     - |      6 KB |
|              GetObservables | 350.3 us | 1.54 us | 1.36 us |  0.97 | 1.4648 |     - |     - |      7 KB |