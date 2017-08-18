namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExtensionsForString
	{
		/// <summary>
		/// Removes one or more instances of a suffix from a string.
		/// </summary>
		public static string RemoveSuffix(this string instance, string suffix)
		{
			Helpers.Argument.ValidateIsNotNull(instance, nameof(instance));
			Helpers.Argument.ValidateIsNotNullOrEmpty(suffix, nameof(suffix));

			while (instance.EndsWith(suffix))
				instance = instance.Substring(0, instance.Length - suffix.Length);

			return instance;
		}

		/// <summary>
		/// Splits a string into nonempty lines, using either \r or \n as the delimiter.
		/// </summary>
		public static IEnumerable<string> AsNonemptyLines(this string text)
		{
			Helpers.Argument.ValidateIsNotNull(text, nameof(text));

			var position = 0;

			while (position != text.Length)
			{
				var nextLine = text.IndexOfAny(new[] { '\n', '\r' }, position);

				if (nextLine == -1)
				{
					var candidate = text.Substring(position);

					// Empty lines are ignored.
					if (candidate.Length != 0)
						yield return candidate;

					position = text.Length;
				}
				else
				{
					var candidate = text.Substring(position, nextLine - position);

					// Empty lines are ignored.
					if (candidate.Length != 0)
						yield return candidate;

					position = nextLine + 1;
				}
			}
		}
	}
}