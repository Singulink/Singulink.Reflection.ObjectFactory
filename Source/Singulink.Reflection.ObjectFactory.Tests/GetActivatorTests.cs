using System;
using System.CodeDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Reflection.Tests;

[TestClass]
public class GetActivatorTests
{
    [TestMethod]
    public void PrivateParamActivation()
    {
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<PrivateParamCtor>());
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivatorDelegate<Func<string, PrivateParamCtor>>(typeof(PrivateParamCtor)));

        var factory1 = ObjectFactory.GetActivatorByDelegate<Func<string, object>>(typeof(PrivateParamCtor), true);
        var factory2 = ObjectFactory.GetActivator<string, PrivateParamCtor>(true);

        ((PrivateParamCtor)factory1.Invoke("test")).ArgValue.ShouldBe("test");
        factory2.Invoke("test").ArgValue.ShouldBe("test");
    }

    [TestMethod]
    public void PrivateDefaultCtorActivation()
    {
        Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<PrivateDefaultCtor>());

        var factory1 = ObjectFactory.GetActivatorByDelegate<Func<object>>(typeof(PrivateDefaultCtor), true);
        var factory2 = ObjectFactory.GetActivator<PrivateDefaultCtor>(true);

        factory1.Invoke().ShouldBeOfType<PrivateDefaultCtor>();
        factory2.Invoke().ShouldNotBeNull();
    }

    [TestMethod]
    public void TupleActivation()
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
        Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivatorByDelegate<Func<string>>(typeof(PrivateDefaultCtor), true));
        Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivatorDelegate<Func<string, string>>(typeof(PrivateParamCtor)));
    }

    [TestMethod]
    public void ShouldReturnObjects()
    {
        ObjectFactory.CreateInstance<PublicDefaultCtor>().ShouldNotBeNull();
        ObjectFactory.CreateInstance<PrivateDefaultCtor>(true).ShouldNotBeNull();

        ObjectFactory.GetActivator<PublicDefaultCtor>().Invoke().ShouldNotBeNull();
        ObjectFactory.GetActivator<PrivateDefaultCtor>(true).Invoke().ShouldNotBeNull();
        ObjectFactory.GetActivator<string, PublicParamCtor>().Invoke("test").ShouldNotBeNull();

        ObjectFactory.GetActivator<ParamStruct>().Invoke();
        ObjectFactory.GetActivator<long, long, ParamStruct>().Invoke(1, 2).ShouldBe(new ParamStruct(1, 2));
    }

    [TestMethod]
    public void ShouldThrowMissingMethod()
    {
        Should.Throw<MissingMemberException>(() => ObjectFactory.CreateInstance<PrivateDefaultCtor>());
        Should.Throw<MissingMemberException>(() => ObjectFactory.CreateInstance<PrivateParamCtor>(true));
        Should.Throw<MissingMemberException>(() => ObjectFactory.CreateInstance<PublicParamCtor>(true));

        Should.Throw<MissingMemberException>(() => ObjectFactory.GetActivator<PrivateDefaultCtor>());
        Should.Throw<MissingMemberException>(() => ObjectFactory.GetActivator<PrivateParamCtor>());
        Should.Throw<MissingMemberException>(() => ObjectFactory.GetActivator<PublicParamCtor>());

        Should.Throw<MissingMemberException>(() => ObjectFactory.GetActivator<long, ParamStruct>());
    }

    public class PublicDefaultCtor
    {
    }

    public class PublicParamCtor
    {
        public PublicParamCtor(string value) { }
    }

    public class PrivateDefaultCtor
    {
        private PrivateDefaultCtor()
        {
        }
    }

    public class PrivateParamCtor
    {
        public bool InitializerCalled { get; } = true;

        public string ArgValue { get; }

        private PrivateParamCtor(string argValue)
        {
            ArgValue = argValue;
        }
    }

    public struct ParamStruct
    {
        public long Value;
        public long Value2;

        public ParamStruct(long value, long value2)
        {
            Value = value;
            Value2 = value2;
        }
    }
}
