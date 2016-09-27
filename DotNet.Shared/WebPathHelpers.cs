namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Different modes for slash character treatement.
	/// </summary>
	public enum SlashMode
	{
		/// <summary>
		/// Slashes are ignored when combiging URIs - they are stripped from the beginning and end of each fragment
		/// and the fragments are simply joined together with slashes from there on.
		/// </summary>
		IgnoreSlashes,

		/// <summary>
		/// The starting slash of the first fragment is preserved, if it exists.
		/// Otherwise, behavior is the same as <see cref="IgnoreSlashes"/>.
		/// This is the default mode.
		/// </summary>
		InterpretInitialSlash
	}

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Combines URI fragments into a single URI, using <see cref="SlashMode.InterpretInitialSlash"/> slash processing.
		/// 
		/// If the first fragment contains "::/", it is considered the root. Otherwise, the root is an empty string.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if <paramref name="fragments"/> is empty.</exception>
		public static string Combine(this HelpersContainerClasses.WebPath container, params string[] fragments)
		{
			return Helpers.WebPath.Combine(SlashMode.InterpretInitialSlash, fragments);
		}

		/// <summary>
		/// Combines URI fragments into a single URI.
		/// 
		/// If the first fragment contains "::/", it is considered the root. Otherwise, the root is an empty string.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if <paramref name="fragments"/> is empty.</exception>
		public static string Combine(this HelpersContainerClasses.WebPath container, SlashMode slashMode, params string[] fragments)
		{
			if (fragments.Length == 0)
				throw new ArgumentException("No fragments provided.", nameof(fragments));

			var remaining = new Queue<string>(fragments);
			var result = new StringBuilder();
			string root;

			var firstFragment = remaining.Peek();
			if (firstFragment.Contains("::/"))
			{
				root = firstFragment.TrimEnd('/');
				remaining.Dequeue();
			}
			else
			{
				root = "";

				if (slashMode == SlashMode.InterpretInitialSlash && firstFragment.StartsWith("/"))
					result.Append(remaining.Dequeue().TrimEnd('/'));
			}

			result.Append(root);

			while (remaining.Count != 0)
			{
				// Skip empty bits.
				var part = remaining.Dequeue().Trim('/');

				if (part == "")
					continue;

				if (result.Length > 0)
					result.Append("/");

				result.Append(part);
			}

			return result.ToString();
		}
	}
}