namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Port of the most basic bare-bones DI functionality to WP7.
	/// </summary>
	public sealed class InjectionContainer
	{
		/// <summary>
		/// Registers the TObject type, providing with a factory method used to create instances.
		/// Overwrites an existing registration, if one is present.
		/// </summary>
		public void RegisterType<TObject>(Func<InjectionContainer, TObject> factory)
			where TObject : class
		{
			Helpers.Argument.ValidateIsNotNull(factory, "factory");

			// Wrap it with an object-producing lambda, since we forget the type for the factory.
			_factories[typeof(TObject)] = container => factory(container);
		}

		/// <summary>
		/// Registers the objectType type, providing with a factory method used to create instances.
		/// Overwrites an existing registration, if one is present.
		/// </summary>
		public void RegisterType(Type objectType, Func<InjectionContainer, object> factory)
		{
			Helpers.Argument.ValidateIsNotNull(objectType, "objectType");
			Helpers.Argument.ValidateIsNotNull(factory, "factory");

			// Wrap it with an object-producing lambda, since we forget the type for the factory.
			_factories[objectType] = container => factory(container);
		}

		/// <summary>
		/// Registers a singleton instance of the TObject type.
		/// Overwrites an existing registration, if one is present.
		/// </summary>
		public void RegisterInstance<TObject>(TObject instance)
			where TObject : class
		{
			Helpers.Argument.ValidateIsNotNull(instance, "instance");

			_factories[typeof(TObject)] = container => instance;
		}

		/// <summary>
		/// Registers a singleton instance of the objectType type.
		/// Overwrites an existing registration, if one is present.
		/// </summary>
		public void RegisterInstance(Type objectType, object instance)
		{
			Helpers.Argument.ValidateIsNotNull(objectType, "objectType");
			Helpers.Argument.ValidateIsNotNull(instance, "instance");

			_factories[objectType] = container => instance;
		}

		/// <summary>
		/// Resolves for an instance of the specified type.
		/// Registered types are used, where possible.
		/// Unexpected types are resolved via default constructor if not registered.
		/// </summary>
		public TObject Resolve<TObject>()
		{
			return (TObject)Resolve(typeof(TObject));
		}

		/// <summary>
		/// Resolves for an instance of the specified reference type.
		/// Registered types are used, where possible. Unexpected types are resolved via longest constructor if not registered.
		/// </summary>
		public object Resolve(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			var typeInfo = type.GetTypeInfo();

			if (!typeInfo.IsClass && !typeInfo.IsInterface)
				throw new ArgumentException("Can only resolve classes or interfaces.");

			object instance = TryResolveRegistered(type);

			if (instance != null)
				return instance;

			if (typeInfo.IsInterface)
				throw new ArgumentException("Cannot resolve unregistered interface.");

			// Not found in singleton or factory list. Create it.
			return ResolveDefault(type);
		}

		/// <summary>
		/// Creates a child container - registrations on a child container only affect the child container,
		/// whereas it will inherit all registrations of the parent container.
		/// </summary>
		public InjectionContainer CreateChildContainer()
		{
			return new InjectionContainer(this);
		}

		#region Initialization
		public InjectionContainer()
		{
			RegisterInstance(this);
		}

		private InjectionContainer(InjectionContainer parent)
		{
			_parent = parent;

			RegisterInstance(this);
		}
		#endregion

		#region Implementation details
		private readonly InjectionContainer _parent;

		private static ConstructorInfo GetLongestConstructor(Type type)
		{
			return type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
		}

		private readonly Dictionary<Type, Func<InjectionContainer, object>> _factories = new Dictionary<Type, Func<InjectionContainer, object>>();

		private object TryResolveRegistered(Type type)
		{
			// Local first, parent later - so we allow local override.

			Func<InjectionContainer, object> factory;

			if (_factories.TryGetValue(type, out factory))
				return factory(this);

			object instance = null;

			if (_parent != null)
				instance = _parent.TryResolveRegistered(type);

			return instance;
		}

		private object ResolveDefault(Type type)
		{
			var constructor = GetLongestConstructor(type);

			if (constructor == null)
				throw new ArgumentException(string.Format("Cannot resolve {0}: no public constructor found.", type.FullName), "type");

			var parameterInfo = constructor.GetParameters();

			object[] parameters = new object[parameterInfo.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				var pType = parameterInfo[i].ParameterType;

				var pTypeInfo = pType.GetTypeInfo();

				if (!pTypeInfo.IsClass && !pTypeInfo.IsInterface)
					throw new ArgumentException(string.Format("Cannot resolve constructor parameter for {0}: {1} is not a class or interface.", type.FullName, pType.FullName));

				try
				{
					parameters[i] = Resolve(pType);
				}
				catch (Exception ex)
				{
					throw new ArgumentException(string.Format("Cannot resolve constructor parameter for {0}: {1}.", type.FullName, pType.FullName), ex);
				}
			}

			return constructor.Invoke(parameters);
		}
		#endregion
	}
}