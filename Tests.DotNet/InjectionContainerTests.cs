namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public class InjectionContainerTests
	{
		private readonly InjectionContainer _container = new InjectionContainer();

		[Fact]
		public void DefaultConstructorIsUsed()
		{
			_container.Resolve<InjectionTestObject1>();
		}

		[Fact]
		public void LongestConstructorIsUsed()
		{
			var instance = _container.Resolve<InjectionTestObject2>();

			Assert.Equal("long", instance.UsedConstructor);
		}

		[Fact]
		public void SingletonIsSingleton()
		{
			var instance = new InjectionTestObject1();

			_container.RegisterInstance(instance);

			Assert.Same(instance, _container.Resolve<InjectionTestObject1>());
		}

		[Fact]
		public void ValueTypeIsNotResolved()
		{
			Assert.Throws<ArgumentException>(() => _container.Resolve<int>());
		}

		[Fact]
		public void NotSingletonIsNotSingleton()
		{
			_container.RegisterType(c => new object());

			Assert.NotSame(_container.Resolve<object>(), _container.Resolve<object>());
		}

		[Fact]
		public void CanOverwriteRegistration1()
		{
			_container.RegisterType(c => new InjectionTestObject1());
			_container.RegisterType(c => new InjectionTestObject1());
		}

		[Fact]
		public void CanOverwriteRegistration2()
		{
			_container.RegisterInstance(new InjectionTestObject1());
			_container.RegisterInstance(new InjectionTestObject1());
		}

		[Fact]
		public void CanOverwriteRegistration3()
		{
			_container.RegisterType(c => new InjectionTestObject1());
			_container.RegisterInstance(new InjectionTestObject1());
		}

		[Fact]
		public void CanOverwriteRegistration4()
		{
			_container.RegisterInstance(new InjectionTestObject1());
			_container.RegisterType(c => new InjectionTestObject1());
		}

		[Fact]
		public void ContainerResolvesItself()
		{
			Assert.Same(_container, _container.Resolve<InjectionContainer>());
		}

		[Fact]
		public void ChildIsSeparateFromParent()
		{
			var child = _container.CreateChildContainer();

			Assert.Same(child, child.Resolve<InjectionContainer>());
			Assert.Same(_container, _container.Resolve<InjectionContainer>());
			Assert.NotSame(_container, child);
		}

		[Fact]
		public void ChildDoesNotContaminateParent()
		{
			var child = _container.CreateChildContainer();

			var instance = new InjectionTestObject1();
			child.RegisterInstance(instance);

			Assert.NotSame(instance, _container.Resolve<InjectionTestObject1>());

			child.RegisterType(c => new InjectionTestObject2());
			Assert.Equal("short", child.Resolve<InjectionTestObject2>().UsedConstructor);
			Assert.Equal("long", _container.Resolve<InjectionTestObject2>().UsedConstructor);
		}

		[Fact]
		public void ChildOverridesParent()
		{
			var singleton = new InjectionTestObject1();
			_container.RegisterInstance(singleton);

			var child = _container.CreateChildContainer();
			child.RegisterType(c => new InjectionTestObject1());

			Assert.Same(_container.Resolve<InjectionTestObject1>(), _container.Resolve<InjectionTestObject1>());
			Assert.NotSame(child.Resolve<InjectionTestObject1>(), child.Resolve<InjectionTestObject1>());
		}

		[Fact]
		public void InterfaceIsDirectlyResolved()
		{
			_container.RegisterType<IInjectionTestInterface1>(c => new InjectionTestObject1());

			var instance = _container.Resolve<IInjectionTestInterface1>();

			Assert.NotNull(instance);
		}

		[Fact]
		public void InterfaceIsIndirectlyResolved()
		{
			_container.RegisterType<IInjectionTestInterface1>(c => new InjectionTestObject1());

			var instance = _container.Resolve<InjectionTestObject3>();

			Assert.NotNull(instance.Item);
		}

		[Fact]
		public void InterfaceIsNotDirectlyResolvedWithoutRegistration()
		{
			Assert.Throws<ArgumentException>(() => _container.Resolve<IInjectionTestInterface1>());
		}

		[Fact]
		public void InterfaceIsNotIndirectlyResolvedWithoutRegistration()
		{
			Assert.Throws<ArgumentException>(() => _container.Resolve<InjectionTestObject3>());
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