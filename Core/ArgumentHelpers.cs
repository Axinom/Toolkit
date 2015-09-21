namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class CoreHelpers
	{
		public static void ValidateIsNotNull<T>(this HelpersContainerClasses.Argument container, T value, string name)
		{
			if (value == null)
				throw new ArgumentNullException(name);
		}

		public static void ValidateIsNotNullOrEmpty<T>(this HelpersContainerClasses.Argument container, T[] value, string name)
		{
			Helpers.Argument.ValidateIsNotNull(value, name);

			if (value.Length == 0)
				throw new ArgumentException("Array must not be empty", name);
		}

		public static void ValidateIsNotNullOrWhitespace(this HelpersContainerClasses.Argument container, string value, string name)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException("Value cannot be null or a string consisting of whitespace.", name);
		}

		public static void ValidateIsNotNullOrEmpty(this HelpersContainerClasses.Argument container, string value, string name)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Value cannot be null or an empty string.", name);
		}

		public static void ValidateRange<T>(this HelpersContainerClasses.Argument container, T value, string name, T? min = null, T? max = null)
			where T : struct, IComparable
		{
			if (min != null)
			{
				if (value.CompareTo(min) == -1)
					throw new ArgumentOutOfRangeException(name, string.Format("Cannot be less than {0}", min));
			}

			if (max != null)
			{
				if (value.CompareTo(max) == 1)
					throw new ArgumentOutOfRangeException(name, string.Format("Cannot be greater than {0}", max));
			}
		}

		public static void ValidateLength(this HelpersContainerClasses.Argument container, ICollection collection, string name, int? exact = null, int? min = null, int? max = null)
		{
			Helpers.Argument.ValidateIsNotNull(collection, name);

			if (exact != null)
			{
				if (collection.Count != exact)
					throw new ArgumentException(string.Format("Collection must have {0} elements.", exact), name);
			}

			if (min != null)
			{
				if (collection.Count < min)
					throw new ArgumentException(string.Format("Collection must have at least {0} elements.", min), name);
			}

			if (max != null)
			{
				if (collection.Count > max)
					throw new ArgumentException(string.Format("Collection must have no more than {0} elements.", max), name);
			}
		}

		public static void ValidateEnum<T>(this HelpersContainerClasses.Argument container, T value, string name)
			where T : struct
		{
			if (!Enum.IsDefined(value.GetType(), value))
				throw new ArgumentOutOfRangeException(name, "Undefined enumeration value.");
		}

		public static void ValidateIsAbsoluteUrl(this HelpersContainerClasses.Argument container, Uri value, string name)
		{
			Helpers.Argument.ValidateIsNotNull(value, name);

			if (!value.IsAbsoluteUri)
				throw new ArgumentException("URL must be absolute.", name);
		}
	}
}