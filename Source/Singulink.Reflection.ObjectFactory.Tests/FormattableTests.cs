using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Reflection.Tests
{
    [TestClass]
    public class FormattableTests
    {
        [TestMethod]
        public void CallsPrivateConstructor()
        {
            var factory1 = ObjectFactory.GetFormattableObjectFactory(typeof(PrivateDefaultConstructor));
            var factory2 = ObjectFactory.GetFormattableObjectFactory<PrivateDefaultConstructor>();

            ((PrivateDefaultConstructor)factory1.Invoke()).InitializerCalled.ShouldBe(true);
            factory2.Invoke().InitializerCalled.ShouldBe(true);
        }

        [TestMethod]
        public void GetsUninitialized()
        {
            var factory1 = ObjectFactory.GetFormattableObjectFactory(typeof(NoDefaultConstructor));
            var factory2 = ObjectFactory.GetFormattableObjectFactory<NoDefaultConstructor>();

            ((NoDefaultConstructor)factory1.Invoke()).InitializerCalled.ShouldBe(false);
            factory2.Invoke().InitializerCalled.ShouldBe(false);
        }

        [TestMethod]
        public void InvalidReturnType()
        {
            Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetFormattableObjectFactory<string>(typeof(PrivateDefaultConstructor)));
            Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetFormattableObjectFactory<string>(typeof(NoDefaultConstructor)));
        }
    }
}
