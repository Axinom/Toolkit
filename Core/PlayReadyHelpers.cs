namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Generates a PlayReady Rights Management Header for the provided key ID.
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
	}
}