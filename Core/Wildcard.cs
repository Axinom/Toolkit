namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	/// <summary>
	/// A <i>*.jpeg</i> style wildcard matcher running on the <see cref="System.Text.RegularExpressions"/> engine.
	/// Supports both the ? and * metacharacters in the wildcard expression.
	/// </summary>
	/// <example>
	/// <code><![CDATA[
	/// Wildcard wildcard = new Wildcard("*.exe");
	/// 
	///	foreach (string filename in Directory.GetFiles(@"C:\Windows"))
	///	{
	///		if (wildcard.IsMatch(filename))
	///			Console.WriteLine("Match: {0}", filename);
	///	}
	/// ]]></code>
	/// </example>
	public sealed class Wildcard : Regex
	{
		/// <summary>
		/// Initializes a wildcard with the given search pattern.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match. ? and * metacharacters are supported.</param>
		public Wildcard(string pattern) : base(WildcardToRegex(pattern))
		{
		}

		/// <summary>
		/// Initializes a wildcard with the given search pattern and options.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match. ? and * metacharacters are supported.</param>
		/// <param name="options">A combination of one or more <see cref="RegexOptions"/>.</param>
		public Wildcard(string pattern, RegexOptions options)
			: base(WildcardToRegex(pattern), options)
		{
		}

		/// <summary>
		/// Converts a wildcard to a regex.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to convert.</param>
		/// <returns>A regex equivalent of the given wildcard.</returns>
		private static string WildcardToRegex(string pattern)
		{
			return "^" + Escape(pattern).
				Replace("\\*", ".*").
				Replace("\\?", ".") + "$";
		}
	}
}