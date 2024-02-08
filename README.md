# Singulink.Reflection.ObjectFactory

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Reflection.ObjectFactory.svg)](https://www.nuget.org/packages/Singulink.Reflection.ObjectFactory/)
[![Build and Test](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/actions?query=workflow%3A%22build+and+test%22)

**ObjectFactory** provides methods to create objects and get delegates that call object constructors with matching parameters. Delegates are cached so that they are shared across the application wherever they are requested. Performance is significantly improved over `System.Activator` (benchmarks below), providing over 100x speed improvement in some cases when parameters are involved.

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The package is available on NuGet - simply install the `Singulink.Reflection.ObjectFactory` package.

**Supported Runtimes**: Anywhere .NET Standard 2.0+ is supported, including:
- .NET Core 2.0+
- .NET Framework 4.6.1+
- Mono 5.4+
- Xamarin.iOS 10.14+
- Xamarin.Android 8.0+

## API

You can view the API on [FuGet](https://www.fuget.org/packages/Singulink.Reflection.ObjectFactory). 

## Benchmarks

**.NET 8**
```
| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_Object                         |  14.9644 ns | 0.2901 ns | 0.3341 ns | 0.0057 |      24 B |
| ObjectFactory_Object                     |   6.5780 ns | 0.0587 ns | 0.0549 ns | 0.0057 |      24 B |
| ObjectFactory_Object_Delegate            |   6.3640 ns | 0.0508 ns | 0.0451 ns | 0.0057 |      24 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_ObjectPrivateCtor              |  11.9688 ns | 0.0894 ns | 0.0793 ns | 0.0057 |      24 B |
| ObjectFactory_ObjectPrivateCtor          |   6.6406 ns | 0.0857 ns | 0.0760 ns | 0.0057 |      24 B |
| ObjectFactory_ObjectPrivateCtor_Delegate |   6.3639 ns | 0.0569 ns | 0.0532 ns | 0.0057 |      24 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_ObjectWithParam                | 285.5964 ns | 2.1783 ns | 1.9310 ns | 0.0858 |     360 B |
| ObjectFactory_ObjectWithParam_Delegate   |   6.5055 ns | 0.1480 ns | 0.1384 ns | 0.0057 |      24 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_Struct                         |   0.1714 ns | 0.0382 ns | 0.0357 ns |      - |         - |
| ObjectFactory_Struct                     |   0.1339 ns | 0.0159 ns | 0.0149 ns |      - |         - |
| ObjectFactory_Struct_Delegate            |   0.4148 ns | 0.0089 ns | 0.0070 ns |      - |         - |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_StructWithParams               | 277.3095 ns | 1.3876 ns | 1.2300 ns | 0.0858 |     360 B |
| ObjectFactory_StructWithParams_Delegate  |   1.7756 ns | 0.0062 ns | 0.0051 ns |      - |         - |
```

**.NET 8 Native AOT**
```
| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_Object                         |   5.8690 ns | 0.1876 ns | 0.2920 ns | 0.0057 |      24 B |
| ObjectFactory_Object                     |   5.4405 ns | 0.1007 ns | 0.0942 ns | 0.0057 |      24 B |
| ObjectFactory_Object_Delegate            |   5.8968 ns | 0.0417 ns | 0.0390 ns | 0.0057 |      24 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_ObjectPrivateCtor              |  93.0655 ns | 0.7956 ns | 0.7052 ns | 0.0057 |      24 B |
| ObjectFactory_ObjectPrivateCtor          |  77.0504 ns | 0.6919 ns | 0.6134 ns | 0.0324 |     136 B |
| ObjectFactory_ObjectPrivateCtor_Delegate |  76.3781 ns | 0.9018 ns | 0.8435 ns | 0.0324 |     136 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_ObjectWithParam                | 365.1306 ns | 2.5832 ns | 2.4163 ns | 0.0763 |     320 B |
| ObjectFactory_ObjectWithParam_Delegate   | 125.5081 ns | 1.9267 ns | 1.8022 ns | 0.0496 |     208 B |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_Struct                         |   0.1400 ns | 0.0131 ns | 0.0116 ns |      - |         - |
| ObjectFactory_Struct                     |   0.1355 ns | 0.0073 ns | 0.0061 ns |      - |         - |
| ObjectFactory_Struct_Delegate            |   1.7573 ns | 0.0323 ns | 0.0302 ns |      - |         - |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Activator_StructWithParams               | 344.7916 ns | 6.7640 ns | 7.5182 ns | 0.0935 |     392 B |
| ObjectFactory_StructWithParams_Delegate  | 193.1942 ns | 1.5188 ns | 1.4207 ns | 0.0706 |     296 B |
```

**.NET Framework**
```
| Method                                   | Mean         | Error      | StdDev     | Gen0   | Allocated |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Activator_Object                         |    72.919 ns |  1.5117 ns |  2.8762 ns | 0.0057 |      24 B |
| ObjectFactory_Object                     |    12.983 ns |  0.2200 ns |  0.1837 ns | 0.0057 |      24 B |
| ObjectFactory_Object_Delegate            |    12.500 ns |  0.1472 ns |  0.1229 ns | 0.0057 |      24 B |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Activator_ObjectPrivateCtor              |    57.444 ns |  1.0517 ns |  1.8420 ns | 0.0057 |      24 B |
| ObjectFactory_ObjectPrivateCtor          |    13.601 ns |  0.3442 ns |  0.5152 ns | 0.0057 |      24 B |
| ObjectFactory_ObjectPrivateCtor_Delegate |    12.489 ns |  0.1248 ns |  0.0975 ns | 0.0057 |      24 B |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Activator_ObjectWithParam                |   966.560 ns |  9.5869 ns |  8.0055 ns | 0.1030 |     433 B |
| ObjectFactory_ObjectWithParam_Delegate   |    11.007 ns |  0.1479 ns |  0.1311 ns | 0.0057 |      24 B |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Activator_Struct                         |    54.208 ns |  0.6618 ns |  0.5867 ns | 0.0076 |      32 B |
| ObjectFactory_Struct                     |     4.110 ns |  0.1152 ns |  0.1233 ns |      - |         - |
| ObjectFactory_Struct_Delegate            |     3.207 ns |  0.0541 ns |  0.0506 ns |      - |         - |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Activator_StructWithParams               | 1,124.746 ns | 15.8511 ns | 13.2364 ns | 0.1202 |     505 B |
| ObjectFactory_StructWithParams_Delegate  |     8.977 ns |  0.1744 ns |  0.1362 ns |      - |         - |
```

This is the benchmark code used to generate the above results:

```cs
private readonly DefaultActivator<SomeObject> _objectFactory = ObjectFactory.GetActivator<SomeObject>();
private readonly DefaultActivator<SomeObjectPrivateCtor> _objectFactoryPrivateCtor = ObjectFactory.GetActivator<SomeObjectPrivateCtor>(true);
private readonly Func<string, SomeObject> _objectFactoryWithParam = ObjectFactory.GetActivator<string, SomeObject>();
private readonly DefaultActivator<SomeStruct> _structFactory = ObjectFactory.GetActivator<SomeStruct>();
private readonly Func<long, long, SomeStruct> _structFactoryWithParams = ObjectFactory.GetActivator<long, long, SomeStruct>();

[Benchmark]
public SomeObject Activator_Object()
{
    return Activator.CreateInstance<SomeObject>();
}

[Benchmark]
public SomeObject ObjectFactory_Object()
{
    return ObjectFactory.CreateInstance<SomeObject>();
}

[Benchmark]
public SomeObject ObjectFactory_Object_Delegate()
{
    return _objectFactory.Invoke();
}

[Benchmark]
public SomeObjectPrivateCtor Activator_ObjectPrivateCtor()
{
    return (SomeObjectPrivateCtor)Activator.CreateInstance(typeof(SomeObjectPrivateCtor), true)!;
}

[Benchmark]
public SomeObjectPrivateCtor ObjectFactory_ObjectPrivateCtor()
{
    return ObjectFactory.CreateInstance<SomeObjectPrivateCtor>(true);
}

[Benchmark]
public SomeObjectPrivateCtor ObjectFactory_ObjectPrivateCtor_Delegate()
{
    return _objectFactoryPrivateCtor.Invoke();
}

[Benchmark]
public SomeObject Activator_ObjectWithParam()
{
    return (SomeObject)Activator.CreateInstance(typeof(SomeObject), "test")!;
}

[Benchmark]
public SomeObject ObjectFactory_ObjectWithParam_Delegate()
{
    return _objectFactoryWithParam.Invoke("test");
}

[Benchmark]
public SomeStruct Activator_Struct()
{
    return Activator.CreateInstance<SomeStruct>();
}

[Benchmark]
public SomeStruct ObjectFactory_Struct()
{
    return ObjectFactory.CreateInstance<SomeStruct>();
}

[Benchmark]
public SomeStruct ObjectFactory_Struct_Delegate()
{
    return _structFactory.Invoke();
}

[Benchmark]
public SomeStruct Activator_StructWithParams()
{
    return (SomeStruct)Activator.CreateInstance(typeof(SomeStruct), 1L, 2L)!;
}

[Benchmark]
public SomeStruct ObjectFactory_StructWithParams_Delegate()
{
    return _structFactoryWithParams.Invoke(1, 2);
}

public class SomeObject
{
    public SomeObject() { }

    public SomeObject(string value) { }
}

public class SomeObjectPrivateCtor
{
    private SomeObjectPrivateCtor() { }
}

public struct SomeStruct
{
    public long Value;
    public long Value2;

    public SomeStruct(long value, long value2)
    {
        Value = value;
        Value2 = value2;
    }
}
```