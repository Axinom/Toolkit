namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;

	public static partial class DotNetHelpers
	{
		/// <summary>
		/// Returns a new cryptographically secure GUID. The returned GUID shall be GUID v4 compatible.
		/// </summary>
		public static Guid NewCryptographicallySecureGuid(this HelpersContainerClasses.Guid container)
		{
			var guidBytes = new byte[16];

			using (var randomGenerator = new RNGCryptoServiceProvider())
				randomGenerator.GetBytes(guidBytes);

			// The 16 bytes in guidBytes might not conform to GUIDv4 format.
			// In order to achieve that, the 8th and 9th byte need to be modified
			// to be in the form of 0x4Y and 0x[8-B]Y respectively where Y stays intact.

			guidBytes[7] &= 0x0F; // Clear the 4 high bits of the 8th byte.
			guidBytes[7] |= 0x40; // Set the 7th bit.

			guidBytes[8] &= 0x3F; // Clear the 2 high bits of the 9th byte.
			guidBytes[8] |= 0x80; // Set the 8th bit.

			return new Guid(guidBytes);
		}
	}
}