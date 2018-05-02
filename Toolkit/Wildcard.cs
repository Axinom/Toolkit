namespace Axinom.Toolkit
{
    using System;
    using System.Text;
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
        /// <param name="pattern">The wildcard pattern to match. ? and * metacharacters are supported.</param>
        public Wildcard(string pattern) : base(WildcardToRegex(pattern))
        {
        }

        /// <param name="patterns">One or more wildcard patterns to match. ? and * metacharacters are supported.</param>
        public Wildcard(params string[] patterns) : base(WildcardToRegex(patterns))
        {
        }

        /// <param name="pattern">The wildcard pattern to match. ? and * metacharacters are supported.</param>
        /// <param name="options">A combination of one or more <see cref="RegexOptions"/>.</param>
        public Wildcard(string pattern, RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        {
        }

        /// <param name="patterns">One or more wildcard patterns to match. ? and * metacharacters are supported.</param>
        /// <param name="options">A combination of one or more <see cref="RegexOptions"/>.</param>
        public Wildcard(string[] patterns, RegexOptions options)
            : base(WildcardToRegex(patterns), options)
        {
        }

        private static string WildcardToRegexVariant(string pattern)
        {
            return Escape(pattern).
                Replace("\\*", ".*").
                Replace("\\?", ".");
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + WildcardToRegexVariant(pattern) + "$";
        }

        private static string WildcardToRegex(string[] patterns)
        {
            if (patterns.Length == 0)
                throw new ArgumentException("At least one pattern must be specified to define a wildcard match.", nameof(patterns));

            var sb = new StringBuilder();
            sb.Append("^(?:");

            for (var i = 0; i < patterns.Length; i++)
            {
                sb.Append(WildcardToRegexVariant(patterns[i]));

                if (i != patterns.Length - 1)
                    sb.Append('|');
            }

            sb.Append(")$");

            return sb.ToString();
        }
    }
}