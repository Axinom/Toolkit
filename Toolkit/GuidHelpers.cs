namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
    using System.Security.Cryptography;

    public static partial class CoreHelpers
	{
		/// <summary>
		/// Generates a GUID that is in timestamp-sotrable order in SQL server.
		/// Use this for generating row IDs that index well.
		/// </summary>
		public static Guid NewCombGuid(this HelpersContainerClasses.Guid container)
		{
			byte[] destinationArray = Guid.NewGuid().ToByteArray();
			DateTime time = new DateTime(0x76c, 1, 1);
			DateTime now = DateTime.UtcNow;
			TimeSpan span = new TimeSpan(now.Ticks - time.Ticks);
			TimeSpan timeOfDay = now.TimeOfDay;
			byte[] bytes = BitConverter.GetBytes(span.Days);
			byte[] array = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
			Array.Reverse(bytes);
			Array.Reverse(array);
			Array.Copy(bytes, bytes.Length - 2, destinationArray, destinationArray.Length - 6, 2);
			Array.Copy(array, array.Length - 4, destinationArray, destinationArray.Length - 4, 4);
			return new Guid(destinationArray);
		}

		/// <summary>
		/// Deserializes a GUID from a byte array that uses the big endian format for all components.
		/// This format is often used by non-Microsoft tooling.
		/// </summary>
		public static Guid FromBigEndianByteArray(this HelpersContainerClasses.Guid container, byte[] bytes)
		{
			Helpers.Argument.ValidateIsNotNull(bytes, "bytes");

			if (!BitConverter.IsLittleEndian)
				throw new InvalidOperationException("This method has not been tested on big endian machines and likely would not operate correctly.");

			var bytesCopy = bytes.ToArray(); // Copy, to ensure that input is not modified.
			Helpers.Guid.FlipSerializedGuidEndianness(bytesCopy);

			return new Guid(bytesCopy);
		}

		public static void FlipSerializedGuidEndianness(this HelpersContainerClasses.Guid container, byte[] bytes)
		{
			// Some encryption tools (e.g. MP4Box) use GUIDs with different byte orders from .NET.
			// Two variants exist: "big endian" and "little endian", with the latter as the defualt for .NET.
			// A GUID consists of 5 groups of bytes, of which only the first 3 are byte order dependent.
			Array.Reverse(bytes, 0, 4);
			Array.Reverse(bytes, 4, 2);
			Array.Reverse(bytes, 6, 2);
		}

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