using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Reflection.Tests;

[TestClass]
public class UninitializedInstanceTests
{
    [TestMethod]
    public void GetsUninitialized()
    {
        var v1 = ObjectFactory.CreateUninitializedInstance(typeof(NoDefaultConstructor));
        var v2 = ObjectFactory.CreateUninitializedInstance<PrivateDefaultConstructor>();

        ((NoDefaultConstructor)v1).InitializerCalled.ShouldBe(false);
        v2.InitializerCalled.ShouldBe(false);
    }

    [TestMethod]
    public void InvalidReturnType()
    {
        Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.CreateUninitializedInstance<string>(typeof(PrivateDefaultConstructor)));
    }
}
