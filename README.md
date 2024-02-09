# Singulink.Reflection.ObjectFactory

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Reflection.ObjectFactory.svg)](https://www.nuget.org/packages/Singulink.Reflection.ObjectFactory/)
[![Build and Test](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/actions?query=workflow%3A%22build+and+test%22)

**ObjectFactory** provides generic methods to create objects and get delegates that call object constructors with matching parameters. Delegates are cached so that they are shared across the application wherever they are requested. Performance has been extensively optimized for all .NET platforms (including Native AOT) and significantly improved over `System.Activator` (benchmarks below), providing over 100x speed improvements in some cases.

### About Singulink

We are a small team of engineers and designers dedicated to building beautiful, functional, and well-engineered software solutions. We offer very competitive rates as well as fixed-price contracts and welcome inquiries to discuss any custom development / project support needs you may have.

This package is part of our **Singulink Libraries** collection. Visit https://github.com/Singulink to see our full list of publicly available libraries and other open-source projects.

## Installation

The package is available on NuGet - simply install the `Singulink.Reflection.ObjectFactory` package.

**Supported Runtimes**: Anywhere .NET Standard 2.0+ is supported, including:
- .NET
- .NET Framework
- Mono
- Xamarin

## API

You can view the API on [FuGet](https://www.fuget.org/packages/Singulink.Reflection.ObjectFactory). 

## Benchmarks

Entries with an `_Activator` suffix use `System.Activator` and the entries below that with an `_ObjectFactory` suffix use this library for comparison.

### .NET 8

#### Reference Types

| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Object_Activator                         |  14.0557 ns | 0.3502 ns | 0.4429 ns | 0.0057 |      24 B |
| Object_ObjectFactory                     |   6.4743 ns | 0.0699 ns | 0.0653 ns | 0.0057 |      24 B |
| Object_ObjectFactory_Delegate            |   6.2483 ns | 0.1278 ns | 0.1067 ns | 0.0057 |      24 B |
| ---
| ObjectPrivateCtor_Activator              |  12.3806 ns | 0.1191 ns | 0.1114 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory          |   6.5123 ns | 0.1423 ns | 0.1261 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory_Delegate |   6.2050 ns | 0.1144 ns | 0.1070 ns | 0.0057 |      24 B |
| ---
| ObjectWithParam_Activator                | 290.2104 ns | 2.2835 ns | 2.0242 ns | 0.0858 |     360 B |
| ObjectWithParam_ObjectFactory_Delegate   |   6.1892 ns | 0.0906 ns | 0.0707 ns | 0.0057 |      24 B |

#### Value Types

| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Struct_Activator                         |   0.1334 ns | 0.0067 ns | 0.0056 ns |      - |         - |
| Struct_ObjectFactory                     |   0.1406 ns | 0.0072 ns | 0.0063 ns |      - |         - |
| Struct_ObjectFactory_Delegate            |   0.1395 ns | 0.0121 ns | 0.0107 ns |      - |         - |
| ---
| StructWithParams_Activator               | 292.7251 ns | 3.2207 ns | 3.0127 ns | 0.0858 |     360 B |
| StructWithParams_ObjectFactory_Delegate  |   1.7616 ns | 0.0289 ns | 0.0270 ns |      - |         - |

### .NET 8 Native AOT

#### Reference Types

| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Object_Activator                         |   5.5204 ns | 0.1827 ns | 0.2561 ns | 0.0057 |      24 B |
| Object_ObjectFactory                     |   5.4420 ns | 0.0637 ns | 0.0532 ns | 0.0057 |      24 B |
| Object_ObjectFactory_Delegate            |   5.6431 ns | 0.1214 ns | 0.1135 ns | 0.0057 |      24 B |
| ---
| ObjectPrivateCtor_Activator              |  91.9347 ns | 1.0674 ns | 0.8913 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory          |  24.5102 ns | 0.0418 ns | 0.0349 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory_Delegate |  24.0892 ns | 0.2957 ns | 0.2766 ns | 0.0057 |      24 B |
| ---
| ObjectWithParam_Activator                | 337.3210 ns | 1.9566 ns | 1.6339 ns | 0.0763 |     320 B |
| ObjectWithParam_ObjectFactory_Delegate   |  32.2363 ns | 0.2639 ns | 0.2339 ns | 0.0057 |      24 B |

#### Value Types

| Method                                   | Mean        | Error     | StdDev    | Gen0   | Allocated |
|----------------------------------------- |------------:|----------:|----------:|-------:|----------:|
| Struct_Activator                         |   0.1423 ns | 0.0129 ns | 0.0121 ns |      - |         - |
| Struct_ObjectFactory                     |   0.1334 ns | 0.0060 ns | 0.0056 ns |      - |         - |
| Struct_ObjectFactory_Delegate            |   0.2120 ns | 0.0297 ns | 0.0330 ns |      - |         - |
| ---
| StructWithParams_Activator               | 317.8065 ns | 1.0806 ns | 0.8437 ns | 0.0935 |     392 B |
| StructWithParams_ObjectFactory_Delegate  |  48.9843 ns | 0.7990 ns | 0.7474 ns | 0.0191 |      80 B |

### .NET Framework 4.8

#### Reference Types

| Method                                   | Mean         | Error      | StdDev     | Gen0   | Allocated |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Object_Activator                         |    64.296 ns |  0.5001 ns |  0.4176 ns | 0.0057 |      24 B |
| Object_ObjectFactory                     |    17.374 ns |  0.1401 ns |  0.1090 ns | 0.0057 |      24 B |
| Object_ObjectFactory_Delegate            |    11.589 ns |  0.2366 ns |  0.2097 ns | 0.0057 |      24 B |
| ---
| ObjectPrivateCtor_Activator              |    57.444 ns |  1.0517 ns |  1.8420 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory          |    15.704 ns |  0.2986 ns |  0.2499 ns | 0.0057 |      24 B |
| ObjectPrivateCtor_ObjectFactory_Delegate |    11.608 ns |  0.2255 ns |  0.2110 ns | 0.0057 |      24 B |
| ---
| ObjectWithParam_Activator                |   898.057 ns |  6.3406 ns |  4.9503 ns | 0.1030 |     433 B |
| ObjectWithParam_ObjectFactory_Delegate   |     9.940 ns |  0.0301 ns |  0.0267 ns | 0.0057 |      24 B |

#### Value Types

| Method                                   | Mean         | Error      | StdDev     | Gen0   | Allocated |
|----------------------------------------- |-------------:|-----------:|-----------:|-------:|----------:|
| Struct_Activator                         |    49.163 ns |  0.3674 ns |  0.3437 ns | 0.0076 |      32 B |
| Struct_ObjectFactory                     |     3.894 ns |  0.0265 ns |  0.0235 ns |      - |         - |
| Struct_ObjectFactory_Delegate            |     3.069 ns |  0.0138 ns |  0.0123 ns |      - |         - |
| ---
| StructWithParams_Activator               | 1,116.467 ns |  4.2834 ns |  4.0067 ns | 0.1202 |     505 B |
| StructWithParams_ObjectFactory_Delegate  |     8.235 ns |  0.0357 ns |  0.0279 ns |      - |         - |

### Benchmark Code

```cs
private readonly DefaultActivator<SomeObject> _objectFactory =
    ObjectFactory.GetActivator<SomeObject>();
private readonly DefaultActivator<SomeObjectPrivateCtor> _objectFactoryPrivateCtor =
    ObjectFactory.GetActivator<SomeObjectPrivateCtor>(true);
private readonly Func<string, SomeObject> _objectFactoryWithParam =
    ObjectFactory.GetActivator<string, SomeObject>();
private readonly DefaultActivator<SomeStruct> _structFactory =
    ObjectFactory.GetActivator<SomeStruct>();
private readonly Func<long, long, SomeStruct> _structFactoryWithParams =
    ObjectFactory.GetActivator<long, long, SomeStruct>();

[Benchmark]
public SomeObject Object_Activator()
{
    return Activator.CreateInstance<SomeObject>();
}

[Benchmark]
public SomeObject Object_ObjectFactory()
{
    return ObjectFactory.CreateInstance<SomeObject>();
}

[Benchmark]
public SomeObject Object_ObjectFactory_Delegate()
{
    return _objectFactory.Invoke();
}

[Benchmark]
public SomeObjectPrivateCtor ObjectPrivateCtor_Activator()
{
    return (SomeObjectPrivateCtor)Activator.CreateInstance(typeof(SomeObjectPrivateCtor), true)!;
}

[Benchmark]
public SomeObjectPrivateCtor ObjectPrivateCtor_ObjectFactory()
{
    return ObjectFactory.CreateInstance<SomeObjectPrivateCtor>(true);
}

[Benchmark]
public SomeObjectPrivateCtor ObjectPrivateCtor_ObjectFactory_Delegate()
{
    return _objectFactoryPrivateCtor.Invoke();
}

[Benchmark]
public SomeObject ObjectWithParam_Activator()
{
    return (SomeObject)Activator.CreateInstance(typeof(SomeObject), "test")!;
}

[Benchmark]
public SomeObject ObjectWithParam_ObjectFactory_Delegate()
{
    return _objectFactoryWithParam.Invoke("test");
}

[Benchmark]
public SomeStruct Struct_Activator()
{
    return Activator.CreateInstance<SomeStruct>();
}

[Benchmark]
public SomeStruct Struct_ObjectFactory()
{
    return ObjectFactory.CreateInstance<SomeStruct>();
}

[Benchmark]
public SomeStruct Struct_ObjectFactory_Delegate()
{
    return _structFactory.Invoke();
}

[Benchmark]
public SomeStruct StructWithParams_Activator()
{
    return (SomeStruct)Activator.CreateInstance(typeof(SomeStruct), 1L, 2L)!;
}

[Benchmark]
public SomeStruct StructWithParams_ObjectFactory_Delegate()
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