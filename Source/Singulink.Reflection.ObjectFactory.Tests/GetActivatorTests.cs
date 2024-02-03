using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Reflection.Tests;

[TestClass]
public class GetActivatorTests
{
    [TestMethod]
    public void GetPrivateNonDefaultActivator()
    {
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<NoDefaultConstructor>());
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivatorDelegate<Func<string, NoDefaultConstructor>>(typeof(NoDefaultConstructor)));

        var factory1 = ObjectFactory.GetActivatorByDelegate<Func<string, object>>(typeof(NoDefaultConstructor), true);
        var factory2 = ObjectFactory.GetActivator<string, NoDefaultConstructor>(true);

        ((NoDefaultConstructor)factory1.Invoke("test")).ArgValue.ShouldBe("test");
        factory2.Invoke("test").ArgValue.ShouldBe("test");
    }

    [TestMethod]
    public void GetPrivateDefaultActivator()
    {
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<PrivateDefaultConstructor>());

        var factory1 = ObjectFactory.GetActivatorByDelegate<Func<object>>(typeof(PrivateDefaultConstructor), true);
        var factory2 = ObjectFactory.GetActivator<PrivateDefaultConstructor>(true);

        ((PrivateDefaultConstructor)factory1.Invoke()).InitializerCalled.ShouldBe(true);
        factory2.Invoke().InitializerCalled.ShouldBe(true);
    }

    [TestMethod]
    public void GetStructActivator()
    {
        var factory = ObjectFactory.GetActivator<(string? Value, int Number)>();
        factory.Invoke().ShouldBe(default);

        var paramFactory = ObjectFactory.GetActivator<string, int, (string? Value, int Number)>();
        paramFactory.Invoke("test", 42).ShouldBe(("test", 42));

        var paramObjFactory = ObjectFactory.GetActivatorDelegate<Func<string, int, object>>(typeof((string?, int)));
        paramObjFactory.Invoke("test", 42).ShouldBe(("test", 42));
    }

    [TestMethod]
    public void InvalidReturnType()
    {
        Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivatorByDelegate<Func<string>>(typeof(PrivateDefaultConstructor), true));
        Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivatorDelegate<Func<string, string>>(typeof(NoDefaultConstructor)));
    }
}
