namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Windows GUI puts some garbage in the beginning of the certificate thumbprint.
		/// This takes off that garbage and removes needless whitespace, returning a nice and clean hex string.
		/// </summary>
		public static string CleanWindowsThumbprint(this HelpersContainerClasses.Certificate container, string thumbprint)
		{
			Helpers.Argument.ValidateIsNotNullOrWhitespace(thumbprint, nameof(thumbprint));

			// For whatever reason, Windows puts character 8206 in the beginning of the string in its GUI.
			return thumbprint.Replace(" ", "").Trim((char)8206);
		}
	}
}