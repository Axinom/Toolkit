namespace Axinom.Toolkit
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public static partial class DotNetHelpers
    {
        /// <summary>
        /// Gets a type from any loaded .NET assembly by its full namespace-qualified name.
        /// Use this for dynamic type loading if you do not know the fully qualified assembly name (or do not want to use it).
        /// 
        /// You may also provide a fully-qualified type name, in which case the specified type is returned.
        /// This allows you to ignore the qualification level of input data, which is the optimal scenario.
        /// 
        /// Returns null if the type is not found.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeName"/> is null.</exception>
        /// <remarks>
        /// Obviously, you're in trouble if you have multiple assemblies that both contain a type with the same name.
        /// </remarks>
        public static Type GetTypeFromAnyAssembly(this HelpersContainerClasses.Type container, string typeName)
        {
            return Helpers.Type.GetTypeFromAnyAssembly(typeName, false);
        }

        /// <summary>
        /// Gets a type from any loaded .NET assembly by its full namespace-qualified name.
        /// Use this for dynamic type loading if you do not know the fully qualified assembly name (or do not want to use it).
        /// 
        /// You may also provide a fully-qualified type name, in which case the specified type is returned.
        /// This allows you to ignore the qualification level of input data, which is the optimal scenario.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="throwOnError"/> is true, is thrown if typeName refers to an unknown type not in any assembly.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="typeName"/> is null.</exception>
        /// <remarks>
        /// Obviously, you're in trouble if you have multiple assemblies that both contain a type with the same name.
        /// </remarks>
        public static Type GetTypeFromAnyAssembly(this HelpersContainerClasses.Type container, string typeName, bool throwOnError)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            var firstAttempt = Type.GetType(typeName, false);

            if (firstAttempt != null)
                return firstAttempt;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }

            if (throwOnError)
                throw new InvalidOperationException(string.Format("The type {0} was not found in any loaded assembly.", typeName));

            return null;
        }

        /// <summary>
        /// Checks whether a type has at exactly one instance of a specific attribute type declared on it.
        /// Inheritance chain is checked, as well.
        /// </summary>
        public static bool HasAttribute<TAttribute>(this HelpersContainerClasses.Type container, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.GetCustomAttributes(typeof(TAttribute), true).Length == 1;
        }

        /// <summary>
        /// Gets the single definition of an attribute from a type.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this HelpersContainerClasses.Type container, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (TAttribute)type.GetCustomAttributes(typeof(TAttribute), true).Single();
        }

        /// <summary>
        /// Determines whether a type, passed in by name, is a potentially constructible type.
        /// </summary>
        /// <param name="typeName">
        /// The name of the type. Any level of qualification is accepted - it matches
        /// the standard <see cref="Type.GetType(string)"/> behavior when looking for the type.
        /// Can be null or empty (in which case, the method always returns false).
        /// </param>
        /// <remarks>
        /// A potentially constructible type is one that:
        /// * does not have any unspecified generic parameters;
        /// * is a class or value type;
        /// * is not abstract;
        /// * has at least one public constructor (does not need to be parameterless).
        /// </remarks>
        public static bool IsPotentiallyConstructibleType(this HelpersContainerClasses.Type container, string typeName)
        {
            return Helpers.Type.IsPotentiallyConstructibleType(typeName, null);
        }

        /// <summary>
        /// Determines whether a type, passed in by name, is a constructible type that
        /// also has a specified aspect defined on it (base class or interface).
        /// </summary>
        /// <param name="typeName">
        /// The name of the type in a format acceptable to <see cref="GetTypeFromAnyAssembly(HelpersContainerClasses.Type, string)"/>. 
        /// 
        /// Can be null or empty (in which case, the method always returns false).
        /// </param>
        /// <param name="requiredAspect">
        /// The aspect (base class or interface) which is required on the type. May be null.
        /// </param>
        /// <remarks>
        /// A potentially constructible type is one that:
        /// * does not have any unspecified generic parameters;
        /// * is a class or value type;
        /// * is not abstract;
        /// * has at least one public constructor (does not need to be parameterless).
        /// </remarks>
        public static bool IsPotentiallyConstructibleType(this HelpersContainerClasses.Type container, string typeName, Type requiredAspect)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Trace.WriteLine("IsPotentiallyConstructibleType returning false -> Type is not specified.");
                return false;
            }

            var type = Helpers.Type.GetTypeFromAnyAssembly(typeName);

            if (type == null)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} is unknown.", typeName));
                return false;
            }

            if (type.IsValueType)
                return HasAspect(type, requiredAspect);

            if (!type.IsClass)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} is not a value type nor a class.", typeName));
                return false;
            }

            if (type.IsAbstract)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} is abstract.", typeName));
                return false;
            }

            if (type.ContainsGenericParameters)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} contains unspecified generic parameters.", typeName));
                return false;
            }

            if (type.GetConstructors().Length == 0)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} does not have at least one public constructor.", typeName));
                return false;
            }

            return HasAspect(type, requiredAspect);
        }

        private static bool HasAspect(Type type, Type requiredAspect)
        {
            if (requiredAspect == null)
                return true;

            var hasAspect = requiredAspect.IsAssignableFrom(type);

            if (!hasAspect)
            {
                Trace.WriteLine(string.Format("IsPotentiallyConstructibleType returning false -> Type {0} does not have required aspect {1}.", type.FullName, requiredAspect.FullName));
                return false;
            }

            return true;
        }
    }
}