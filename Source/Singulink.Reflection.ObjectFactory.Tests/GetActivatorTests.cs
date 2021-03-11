using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Reflection.Tests
{
    [TestClass]
    public class GetActivatorTests
    {
        [TestMethod]
        public void GetPrivateNonDefaultActivator()
        {
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<object>(typeof(NoDefaultConstructor)));
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<string, object>(typeof(NoDefaultConstructor)));

            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<NoDefaultConstructor>());
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<string, NoDefaultConstructor>());

            var factory1 = ObjectFactory.GetActivator<string, object>(typeof(NoDefaultConstructor), true);
            var factory2 = ObjectFactory.GetActivator<string, NoDefaultConstructor>(true);

            ((NoDefaultConstructor)factory1.Invoke("test")).ArgValue.ShouldBe("test");
            factory2.Invoke("test").ArgValue.ShouldBe("test");
        }

        [TestMethod]
        public void GetPrivateDefaultActivator()
        {
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator(typeof(PrivateDefaultConstructor)));
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<object>(typeof(PrivateDefaultConstructor)));
            Assert.ThrowsException<MissingMethodException>(() => _ = ObjectFactory.GetActivator<PrivateDefaultConstructor>());

            var factory1 = ObjectFactory.GetActivator(typeof(PrivateDefaultConstructor), true);
            var factory2 = ObjectFactory.GetActivator<PrivateDefaultConstructor>(true);

            ((PrivateDefaultConstructor)factory1.Invoke()).InitializerCalled.ShouldBe(true);
            factory2.Invoke().InitializerCalled.ShouldBe(true);
        }

        [TestMethod]
        public void InvalidReturnType()
        {
            Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivator<string>(typeof(PrivateDefaultConstructor), true));
            Assert.ThrowsException<InvalidCastException>(() => _ = ObjectFactory.GetActivator<string, string>(typeof(NoDefaultConstructor)));
        }
    }
}
