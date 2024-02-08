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
        var v = ObjectFactory.CreateUninitializedInstance<Initializer>();
        v.InitializerCalled.ShouldBe(false);
    }

    public class Initializer
    {
        public bool InitializerCalled { get; } = true;
    }
}
