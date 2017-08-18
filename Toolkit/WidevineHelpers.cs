namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Generates a Widevine CENC header for the provided key ID.
		/// </summary>
		public static byte[] GenerateWidevineCencHeader(this HelpersContainerClasses.Widevine container, Guid keyId)
		{
			using (var buffer = new MemoryStream())
			{
				using (var writer = new BinaryWriter(buffer))
				{
					// We serialize the fields directly in binary to avoid a dependency on protobuf-net.
					// The fields we serialize are algorithmm (1, enum) and key_id (2, bytes).

					const byte wireTypeVarint = 0;
					const byte wireTypeLengthDelimited = 2;

					const byte algorithmAesCtr = 1;

					byte algorithmKey = 1 << 3 | wireTypeVarint;
					byte algorithmValue = algorithmAesCtr;

					byte keyIdKey = 2 << 3 | wireTypeLengthDelimited;
					byte keyIdLength = 16;
					byte[] keyIdBytes = keyId.ToBigEndianByteArray();

					writer.Write(algorithmKey);
					writer.Write(algorithmValue);
					writer.Write(keyIdKey);
					writer.Write(keyIdLength);
					writer.Write(keyIdBytes);
				}

				return buffer.ToArray();
			}
		}
	}
}