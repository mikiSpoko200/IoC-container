using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zadanie1.Tests
{
    [TestClass]
    public class SimpleContainerTests
    {
        [TestMethod]
        public void ResolveSingletonTrue()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<SimpleContainerType>(true);

            SimpleContainerType receivedType = container.Resolve<SimpleContainerType>();
            SimpleContainerType receivedType2 = container.Resolve<SimpleContainerType>();
            Assert.IsTrue(receivedType.Equals(receivedType2));
        }

        [TestMethod]
        public void ResolveSingletonFalse()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<SimpleContainerType>(false);

            SimpleContainerType receivedType = container.Resolve<SimpleContainerType>();
            SimpleContainerType receivedType2 = container.Resolve<SimpleContainerType>();
            Assert.IsFalse(receivedType.Equals(receivedType2));
        }

        [TestMethod]
        public void ResolveSingletonDifferentTypes()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<SimpleContainerType>(false);
            container.RegisterType<SimpleContainerType2>(false);

            SimpleContainerType receivedType = container.Resolve<SimpleContainerType>();
            SimpleContainerType2 receivedType2 = container.Resolve<SimpleContainerType2>();

            Assert.IsFalse(receivedType.Equals(receivedType2));
        }

        [TestMethod]
        public void ResolveImplementationWithSingleton()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<ITested, Tested>(true);

            Tested? returnedType = container.Resolve<ITested>() as Tested;
            Tested? returnedType2 = container.Resolve<ITested>() as Tested;

            Assert.IsNotNull(returnedType);
            Assert.IsNotNull(returnedType2);
            Assert.IsInstanceOfType(returnedType, typeof(Tested));
            Assert.IsInstanceOfType(returnedType, typeof(Tested));
            Assert.IsTrue(returnedType.Equals(returnedType2));
        }

        [TestMethod]
        public void ResolveImplementationWithoutSingleton()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<ITested, Tested>(false);

            Tested? returnedType = container.Resolve<ITested>() as Tested;
            Tested? returnedType2 = container.Resolve<ITested>() as Tested;

            Assert.IsNotNull(returnedType);
            Assert.IsNotNull(returnedType2);
            Assert.IsInstanceOfType(returnedType, typeof(Tested));
            Assert.IsInstanceOfType(returnedType, typeof(Tested));
            Assert.IsFalse(returnedType.Equals(returnedType2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ResolveClassWithoutImplementation()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<ITested>(true);

            ITested returnedType = container.Resolve<ITested>();
        }

        [TestMethod]
        public void ResolveSwitchedImplementation()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<ITested, Tested>(true);
            ITested? returnedType = container.Resolve<ITested>();

            container.RegisterType<ITested, Tested2>(true);
            ITested? returnedType2 = container.Resolve<ITested>();

            Assert.IsNotNull(returnedType);
            Assert.IsNotNull(returnedType2);
            Assert.IsInstanceOfType(returnedType, typeof(Tested));
            Assert.IsInstanceOfType(returnedType2, typeof(Tested2));
        }

        [TestMethod]
        public void ResolveRegisteredInstance()
        {
            SimpleContainer container = new SimpleContainer();
            Tested tested = new Tested();

            container.RegisterInstance<Tested>(tested);
            Assert.AreEqual(tested, container.Resolve<Tested>());
        }

        [TestMethod]
        public void ResolveNotRegisteredInstance()
        {
            SimpleContainer container = new SimpleContainer();
            Tested tested = new Tested();

            Assert.AreNotEqual(tested, container.Resolve<Tested>());
        }

        [TestMethod]
        public void ResolveRegisteredInstanceChange()
        {
            SimpleContainer container = new SimpleContainer();
            Tested tested = new Tested();
            Tested2 tested2 = new Tested2();

            container.RegisterType<ITested, Tested>(false);
            container.RegisterInstance<ITested>(tested);
            Assert.AreEqual(tested, container.Resolve<ITested>());

            container.RegisterType<ITested, Tested2>(false);
            container.RegisterInstance<ITested>(tested2);
            Assert.AreEqual(tested2, container.Resolve<ITested>());
        }

        [TestMethod]
        public void ResolveRegisterInstanceUserDefinedConstructor()
        {
            SimpleContainer container = new SimpleContainer();
            A a = container.Resolve<A>();
            Assert.IsNotNull(a.b);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Class with multiple attributed constructors was inappropriately allowed.")]
        public void ResolveMultipleAttributedConstructorExplicitRegistration()
        {
            SimpleContainer container = new SimpleContainer();
            Tested3 tested = new Tested3();
            container.RegisterInstance<Tested3>(tested);
        }
    }
}
