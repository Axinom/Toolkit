namespace Axinom.Toolkit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static partial class NetStandardHelpers
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

        public static void ValidateIsNotNullOrEmpty<T>(this HelpersContainerClasses.Argument container, ICollection<T> value, string name)
        {
            Helpers.Argument.ValidateIsNotNull(value, name);

            if (value.Count == 0)
                throw new ArgumentException("Collection must not be empty", name);
        }

        public static void ValidateIsNotNullOrEmpty<T>(this HelpersContainerClasses.Argument container, IReadOnlyCollection<T> value, string name)
        {
            Helpers.Argument.ValidateIsNotNull(value, name);

            if (value.Count == 0)
                throw new ArgumentException("Collection must not be empty", name);
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
                    throw new ArgumentOutOfRangeException(name, $"Cannot be less than {min}");
            }

            if (max != null)
            {
                if (value.CompareTo(max) == 1)
                    throw new ArgumentOutOfRangeException(name, $"Cannot be greater than {max}");
            }
        }

        public static void ValidateLength(this HelpersContainerClasses.Argument container, ICollection collection, string name, int? exact = null, int? min = null, int? max = null)
        {
            Helpers.Argument.ValidateIsNotNull(collection, name);

            if (exact != null)
            {
                if (collection.Count != exact)
                    throw new ArgumentException($"Collection must have {exact} elements.", name);
            }

            if (min != null)
            {
                if (collection.Count < min)
                    throw new ArgumentException($"Collection must have at least {min} elements.", name);
            }

            if (max != null)
            {
                if (collection.Count > max)
                    throw new ArgumentException($"Collection must have no more than {max} elements.", name);
            }
        }

        public static void ValidateLength<T>(this HelpersContainerClasses.Argument container, T[] array, string name, int? exact = null, int? min = null, int? max = null)
        {
            ValidateLength(container, (IReadOnlyCollection<T>)array, name, exact, min, max);
        }

        public static void ValidateLength<T>(this HelpersContainerClasses.Argument container, ICollection<T> collection, string name, int? exact = null, int? min = null, int? max = null)
        {
            Helpers.Argument.ValidateIsNotNull(collection, name);

            if (exact != null)
            {
                if (collection.Count != exact)
                    throw new ArgumentException($"Collection must have {exact} elements.", name);
            }

            if (min != null)
            {
                if (collection.Count < min)
                    throw new ArgumentException($"Collection must have at least {min} elements.", name);
            }

            if (max != null)
            {
                if (collection.Count > max)
                    throw new ArgumentException($"Collection must have no more than {max} elements.", name);
            }
        }

        public static void ValidateLength<T>(this HelpersContainerClasses.Argument container, IReadOnlyCollection<T> collection, string name, int? exact = null, int? min = null, int? max = null)
        {
            Helpers.Argument.ValidateIsNotNull(collection, name);

            if (exact != null)
            {
                if (collection.Count != exact)
                    throw new ArgumentException($"Collection must have {exact} elements.", name);
            }

            if (min != null)
            {
                if (collection.Count < min)
                    throw new ArgumentException($"Collection must have at least {min} elements.", name);
            }

            if (max != null)
            {
                if (collection.Count > max)
                    throw new ArgumentException($"Collection must have no more than {max} elements.", name);
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