# Experiments with the Avalonia value store

Trying out a new way of storing values to increase performance of the Avalonia value store.

There's currently a boxing and non-boxing variant.

A probably incomplete todo:

- [ ] Non-local value bindings
- [ ] Animation bindings
- [ ] Template support in setters
- [ ] Property value inhertance
- [ ] Validation/coercion
- [ ] GetBaseValue
- [ ] GetObservable
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

|               Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|----------:|----------:|------:|--------:|--------:|------:|------:|----------:|
| SetClrPropertyValues |   5.762 us | 0.0346 us | 0.0307 us |  1.00 |    0.00 |       - |     - |     - |         - |
|            SetValues | 227.889 us | 1.9449 us | 1.7241 us | 39.55 |    0.37 | 20.9961 |     - |     - |  88,000 B |

|                            Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
| Toggle_Style_Activation_Via_Class | 106.6 us | 0.37 us | 0.33 us | 8.7891 |     - |     - |     36 KB |

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

## Non-Boxing

At 01c6e1f049d428f48dbdfbc83b82976285d5ca5f

|           Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |---------:|----------:|----------:|------:|------:|------:|----------:|
| GetDefaultValues | 5.720 us | 0.0407 us | 0.0361 us |     - |     - |     - |         - |

|               Method |        Mean |     Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |------------:|----------:|---------:|------:|--------:|------:|------:|------:|----------:|
| GetClrPropertyValues |    436.6 ns |   1.28 ns |  1.07 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|            GetValues | 15,197.6 ns | 100.19 ns | 93.72 ns | 34.79 |    0.25 |     - |     - |     - |         - |

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

## Boxing

At 01c6e1f049d428f48dbdfbc83b82976285d5ca5f

|           Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
| GetDefaultValues | 23.79 us | 0.064 us | 0.060 us | 6.8665 |     - |     - |     28 KB |

|               Method |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
| GetClrPropertyValues |   436.9 ns |  4.23 ns |  3.95 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|            GetValues | 5,984.1 ns | 18.26 ns | 16.18 ns | 13.70 |    0.14 |     - |     - |     - |         - |

|               Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|----------:|----------:|------:|--------:|--------:|------:|------:|----------:|
| SetClrPropertyValues |   5.554 us | 0.0287 us | 0.0268 us |  1.00 |    0.00 |       - |     - |     - |         - |
|            SetValues | 289.386 us | 1.6997 us | 1.5068 us | 52.08 |    0.35 | 13.6719 |     - |     - |  57,600 B |

|                            Method |     Mean |   Error |  StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|--------:|--------:|--------:|------:|------:|----------:|
| Setup_Dispose_LocalValue_Bindings | 737.9 us | 3.59 us | 3.19 us | 27.3438 |     - |     - |    112 KB |
|          Fire_LocalValue_Bindings | 299.2 us | 1.22 us | 1.15 us | 14.6484 |     - |     - |     60 KB |

|                            Method |     Mean |    Error |   StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|---------:|---------:|--------:|------:|------:|----------:|
| Toggle_Style_Activation_Via_Class | 52.41 us | 0.137 us | 0.122 us | 18.4937 |     - |     - |     76 KB |

|              Method | MatchingStyles | NonMatchingStyles |       Mean |    Error |   StdDev |     Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |--------------- |------------------ |-----------:|---------:|---------:|-----------:|-------:|------:|------:|----------:|
| Apply_Simple_Styles |              1 |                 1 |   433.3 ns |  1.14 ns |  0.96 ns |   433.1 ns | 0.1431 |     - |     - |     600 B |
| Apply_Simple_Styles |              1 |                 5 |   561.8 ns |  8.52 ns |  7.97 ns |   563.4 ns | 0.1431 |     - |     - |     600 B |
| Apply_Simple_Styles |              1 |                50 | 1,918.1 ns | 24.94 ns | 23.33 ns | 1,913.7 ns | 0.1431 |     - |     - |     600 B |
| Apply_Simple_Styles |              5 |                 1 | 1,005.3 ns | 20.16 ns | 37.86 ns | 1,010.4 ns | 0.1640 |     - |     - |     688 B |
| Apply_Simple_Styles |              5 |                 5 | 1,301.5 ns | 16.07 ns | 15.03 ns | 1,302.9 ns | 0.1640 |     - |     - |     688 B |
| Apply_Simple_Styles |              5 |                50 | 2,403.5 ns | 48.08 ns | 92.64 ns | 2,348.3 ns | 0.1640 |     - |     - |     688 B |
| Apply_Simple_Styles |             50 |                 1 | 7,470.0 ns | 31.98 ns | 29.91 ns | 7,468.7 ns | 0.3891 |     - |     - |   1,656 B |
| Apply_Simple_Styles |             50 |                 5 | 7,455.4 ns | 45.57 ns | 42.63 ns | 7,463.0 ns | 0.3891 |     - |     - |   1,656 B |
| Apply_Simple_Styles |             50 |                50 | 8,883.8 ns | 48.24 ns | 42.77 ns | 8,873.8 ns | 0.3815 |     - |     - |   1,656 B |