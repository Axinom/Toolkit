namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Generates a PlayReady Header for the provided key ID.
		/// </summary>
		public static byte[] GenerateRightsManagementHeader(this HelpersContainerClasses.PlayReady container, Guid keyId)
		{
			var kidString = Convert.ToBase64String(keyId.ToByteArray());

			// Plain text manipulation here to keep things simple. Some common issues include:
			// 1) The first element must be EXACTLY as written here. Including small things like order of attributes.
			// 2) There must be no extra whitespace anywhere.
			var xml = $"<WRMHEADER xmlns=\"http://schemas.microsoft.com/DRM/2007/03/PlayReadyHeader\" version=\"4.0.0.0\"><DATA><PROTECTINFO><KEYLEN>16</KEYLEN><ALGID>AESCTR</ALGID></PROTECTINFO><KID>{kidString}</KID></DATA></WRMHEADER>";

			var xmlBytes = Encoding.Unicode.GetBytes(xml);

			using (var buffer = new MemoryStream())
			{
				using (var writer = new BinaryWriter(buffer))
				{
					// Size (32)
					// RecordCount (16)
					//		RecordType (16)
					//		RecordLength (16)
					//		Data (xml)

					writer.Write(xmlBytes.Length + 4 + 2 + 2 + 2); // Length.
					writer.Write((ushort)1); // Record count.
					writer.Write((ushort)1); // Record type (RM header).
					writer.Write((ushort)xmlBytes.Length); // Record length.
					writer.Write(xmlBytes);
				}

				return buffer.ToArray();
			}
		}

		public static Guid GetKeyIdFromPlayReadyHeader(this HelpersContainerClasses.PlayReady container, byte[] playReadyHeader)
		{
			Helpers.Argument.ValidateIsNotNullOrEmpty(playReadyHeader, nameof(playReadyHeader));

			using (var reader = new BinaryReader(new MemoryStream(playReadyHeader)))
			{
				var headerLength = reader.ReadUInt32();

				if (headerLength != playReadyHeader.Length)
					throw new ArgumentException("PlayReady header does not have the expected size.");

				var recordCount = reader.ReadUInt16();

				for (int recordIndex = 0; recordIndex < recordCount; recordIndex++)
				{
					var recordType = reader.ReadUInt16();
					var recordLength = reader.ReadUInt16();

					// 1 is the Rights Management header.
					if (recordType != 1)
					{
						reader.BaseStream.Position += recordLength;
						continue;
					}

					var rmHeaderBytes = reader.ReadBytesAndVerify(recordLength);
					var rmHeader = Encoding.Unicode.GetString(rmHeaderBytes);

					return Helpers.PlayReady.GetKeyIdFromRightsManagementHeader(rmHeader);
				}
			}

			throw new ArgumentException("PlayReady header does not contain a rights management header.");
		}

		public static Guid GetKeyIdFromRightsManagementHeader(this HelpersContainerClasses.PlayReady container, string rmHeader)
		{
			Helpers.Argument.ValidateIsNotNullOrWhitespace(rmHeader, nameof(rmHeader));

			var document = XDocument.Parse(rmHeader);

			var kidElement = document.Root
				.Element(XName.Get("DATA", PlayReadyConstants.RightsManagementHeaderNamespace))
				?.Element(XName.Get("KID", PlayReadyConstants.RightsManagementHeaderNamespace));

			if (kidElement == null)
				throw new ArgumentException("Rights Management header does not contain the KID element.", nameof(rmHeader));

			var kidString = kidElement.Value;

			return new Guid(Convert.FromBase64String(kidString));
		}
	}
}