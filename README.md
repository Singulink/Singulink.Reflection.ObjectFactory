# Singulink.Reflection.ObjectFactory

[![Chat on Discord](https://img.shields.io/discord/906246067773923490)](https://discord.gg/EkQhJFsBu6)
[![View nuget packages](https://img.shields.io/nuget/v/Singulink.Reflection.ObjectFactory.svg)](https://www.nuget.org/packages/Singulink.Reflection.ObjectFactory/)
[![Build and Test](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/workflows/build%20and%20test/badge.svg)](https://github.com/Singulink/Singulink.Reflection.ObjectFactory/actions?query=workflow%3A%22build+and+test%22)

**ObjectFactory** provides methods to create objects and get delegates that call object constructors with matching parameters. Delegates are cached so that they are shared across the application wherever they are requested. Performance is significantly improved over `System.Activator` (benchmarks below) with a 100x speed improvement when parameters are involved.

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

## Benchmarks (.NET 5.0)

```
|                   Method |       Mean |     Error |    StdDev |  Gen 0 | Allocated |
|------------------------- |-----------:|----------:|----------:|-------:|----------:|
|          ActivatorCreate |  40.113 ns | 0.3501 ns | 0.3103 ns | 0.0057 |      24 B |
| ActivatorCreateWithParam | 607.032 ns | 5.7650 ns | 5.1105 ns | 0.1011 |     424 B |
|            FactoryCreate |  16.547 ns | 0.2585 ns | 0.2292 ns | 0.0057 |      24 B |
|          FactoryDelegate |   6.550 ns | 0.0881 ns | 0.0824 ns | 0.0057 |      24 B |
| FactoryDelegateWithParam |   6.255 ns | 0.0618 ns | 0.0578 ns | 0.0057 |      24 B |
```

```cs
SomeObject _result;

[Benchmark]
public void ActivatorCreate()
{
    _result = Activator.CreateInstance<SomeObject>();
}

[Benchmark]
public void ActivatorCreateWithParam()
{
    _result = (SomeObject)Activator.CreateInstance(typeof(SomeObject), "test");
}

[Benchmark]
public void FactoryCreate()
{
    _result = ObjectFactory.CreateInstance<SomeObject>();
}

Func<SomeObject> _factory = ObjectFactory.GetActivator<SomeObject>();

[Benchmark]
public void FactoryDelegate()
{
    _result = _factory.Invoke();
}

Func<string, SomeObject> _factoryWithParam = ObjectFactory.GetActivator<string, SomeObject>();

[Benchmark]
public void FactoryDelegateWithParam()
{
    _result = _factoryWithParam.Invoke("test");
}

class SomeObject
{
    public SomeObject() { }

    public SomeObject(string value) { }
}
```
