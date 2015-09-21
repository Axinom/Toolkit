namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExtensionsForGuid
	{
		/// <summary>
		/// Serializes the GUID to a byte array, using the big endian format for all components.
		/// This format is often used by non-Microsoft tooling.
		/// </summary>
		public static byte[] ToBigEndianByteArray(this Guid guid)
		{
			Helpers.Argument.ValidateIsNotNull(guid, "guid");

			if (!BitConverter.IsLittleEndian)
				throw new InvalidOperationException("This method has not been tested on big endian machines and likely would not operate correctly.");

			var bytes = guid.ToByteArray();

			Helpers.Guid.FlipSerializedGuidEndianness(bytes);

			return bytes;
		}
	}
}