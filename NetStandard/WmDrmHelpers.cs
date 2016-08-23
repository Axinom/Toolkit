namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// The format string for the full datetime string as used in Windows Media DRM.
		/// Shorter forms are also used, so do not use this for exact parsing.
		/// </summary>
		public static string GetDateTimeLongFormatString(this HelpersContainerClasses.WindowsMediaDrm container)
		{
			return "#yyyyMMdd HH:mm:ssZ#";
		}

		/// <summary>
		/// Decodes a base64 string from the Windows Media DRM base64 format.
		/// </summary>
		public static byte[] Base64Decode(this HelpersContainerClasses.WindowsMediaDrm container, string encoded)
		{
			Helpers.Argument.ValidateIsNotNull(encoded, "encoded");

			encoded = encoded.Replace('!', '+').Replace('*', '/');

			return Convert.FromBase64String(encoded);
		}

		/// <summary>
		/// Encodes a byte array into the Windows Media DRM base64 format.
		/// </summary>
		public static string Base64Encode(this HelpersContainerClasses.WindowsMediaDrm container, byte[] data)
		{
			Helpers.Argument.ValidateIsNotNull(data, "data");

			return Convert.ToBase64String(data).Replace('+', '!').Replace('/', '*');
		}
	}
}