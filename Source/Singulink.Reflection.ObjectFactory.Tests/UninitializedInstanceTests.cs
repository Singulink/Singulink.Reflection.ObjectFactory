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
        var v = ObjectFactory.CreateUninitializedInstance<PrivateDefaultConstructor>();
        v.InitializerCalled.ShouldBe(false);
    }
}
