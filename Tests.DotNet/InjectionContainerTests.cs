namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public class InjectionContainerTests
	{
		private InjectionContainer _container;

		[SetUp]
		public void Initialize()
		{
			_container = new InjectionContainer();
		}

		[Test]
		public void DefaultConstructorIsUsed()
		{
			_container.Resolve<InjectionTestObject1>();
		}

		[Test]
		public void LongestConstructorIsUsed()
		{
			var instance = _container.Resolve<InjectionTestObject2>();

			Assert.AreEqual("long", instance.UsedConstructor);
		}

		[Test]
		public void SingletonIsSingleton()
		{
			var instance = new InjectionTestObject1();

			_container.RegisterInstance(instance);

			Assert.AreSame(instance, _container.Resolve<InjectionTestObject1>());
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ValueTypeIsNotResolved()
		{
			_container.Resolve<int>();
		}

		[Test]
		public void NotSingletonIsNotSingleton()
		{
			_container.RegisterType(c => new object());

			Assert.AreNotSame(_container.Resolve<object>(), _container.Resolve<object>());
		}

		[Test]
		public void CanOverwriteRegistration1()
		{
			_container.RegisterType(c => new InjectionTestObject1());
			_container.RegisterType(c => new InjectionTestObject1());
		}

		[Test]
		public void CanOverwriteRegistration2()
		{
			_container.RegisterInstance(new InjectionTestObject1());
			_container.RegisterInstance(new InjectionTestObject1());
		}

		[Test]
		public void CanOverwriteRegistration3()
		{
			_container.RegisterType(c => new InjectionTestObject1());
			_container.RegisterInstance(new InjectionTestObject1());
		}

		[Test]
		public void CanOverwriteRegistration4()
		{
			_container.RegisterInstance(new InjectionTestObject1());
			_container.RegisterType(c => new InjectionTestObject1());
		}

		[Test]
		public void ContainerResolvesItself()
		{
			Assert.AreSame(_container, _container.Resolve<InjectionContainer>());
		}

		[Test]
		public void ChildIsSeparateFromParent()
		{
			var child = _container.CreateChildContainer();

			Assert.AreSame(child, child.Resolve<InjectionContainer>());
			Assert.AreSame(_container, _container.Resolve<InjectionContainer>());
			Assert.AreNotSame(_container, child);
		}

		[Test]
		public void ChildDoesNotContaminateParent()
		{
			var child = _container.CreateChildContainer();

			var instance = new InjectionTestObject1();
			child.RegisterInstance(instance);

			Assert.AreNotSame(instance, _container.Resolve<InjectionTestObject1>());

			child.RegisterType(c => new InjectionTestObject2());
			Assert.AreEqual("short", child.Resolve<InjectionTestObject2>().UsedConstructor);
			Assert.AreEqual("long", _container.Resolve<InjectionTestObject2>().UsedConstructor);
		}

		[Test]
		public void ChildOverridesParent()
		{
			var singleton = new InjectionTestObject1();
			_container.RegisterInstance(singleton);

			var child = _container.CreateChildContainer();
			child.RegisterType(c => new InjectionTestObject1());

			Assert.AreSame(_container.Resolve<InjectionTestObject1>(), _container.Resolve<InjectionTestObject1>());
			Assert.AreNotSame(child.Resolve<InjectionTestObject1>(), child.Resolve<InjectionTestObject1>());
		}

		[Test]
		public void InterfaceIsDirectlyResolved()
		{
			_container.RegisterType<IInjectionTestInterface1>(c => new InjectionTestObject1());

			var instance = _container.Resolve<IInjectionTestInterface1>();

			Assert.IsNotNull(instance);
		}

		[Test]
		public void InterfaceIsIndirectlyResolved()
		{
			_container.RegisterType<IInjectionTestInterface1>(c => new InjectionTestObject1());

			var instance = _container.Resolve<InjectionTestObject3>();

			Assert.IsNotNull(instance.Item);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void InterfaceIsNotDirectlyResolvedWithoutRegistration()
		{
			_container.Resolve<IInjectionTestInterface1>();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void InterfaceIsNotIndirectlyResolvedWithoutRegistration()
		{
			_container.Resolve<InjectionTestObject3>();
		}
	}

	public class InjectionTestObject1 : IInjectionTestInterface1
	{
	}

	public interface IInjectionTestInterface1
	{
	}

	public class InjectionTestObject2
	{
		public string UsedConstructor { get; set; }

		public InjectionTestObject2()
		{
			UsedConstructor = "short";
		}

		public InjectionTestObject2(InjectionTestObject1 dependency)
		{
			UsedConstructor = "long";
		}
	}

	public class InjectionTestObject3
	{
		public IInjectionTestInterface1 Item { get; set; }

		public InjectionTestObject3(IInjectionTestInterface1 item)
		{
			Item = item;
		}
	}
}