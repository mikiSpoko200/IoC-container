﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zadanie1.Tests
{
    [TestClass]
    public class SimpleContainerTests
    {
        [TestMethod]
        public void RegisterSingletonTrue()
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
            Assert.IsFalse(returnedType.Equals(returnedType2));
            Assert.Equals(returnedType, returnedType2);
        }

        [TestMethod]
        public void ResolveImplementationWithoutSingleton()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<ITested, Tested>(true);

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
            Assert.IsInstanceOfType(returnedType, typeof(Tested2));
        }
    }
}
