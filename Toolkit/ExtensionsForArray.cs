namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExtensionsForArray
	{
		public static bool ContainsArray<TElement>(this TElement[] haystack, TElement[] needle)
		{
			Helpers.Argument.ValidateIsNotNull(haystack, "haystack");
			Helpers.Argument.ValidateIsNotNull(needle, "needle");

			for (var i = 0; i <= haystack.Length - needle.Length; i++)
			{
				for (var j = 0; j < needle.Length; j++)
				{
					if (!haystack[i + j].Equals(needle[j]))
						break;

					// Last element of needle is a match - we have a positive result.
					if (j == needle.Length - 1)
						return true;
				}
			}

			return false;
		}
	}
}