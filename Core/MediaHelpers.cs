namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	public static partial class CoreHelpers
	{
		/// <summary>
		/// Calculates the required crop data in order to crop a picture into the desired aspect ratio.
		/// </summary>
		public static CropResult Crop(this HelpersContainerClasses.Media container, int width, int height, double targetAspectRatio)
		{
			Helpers.Argument.ValidateRange(width, nameof(width), 1);
			Helpers.Argument.ValidateRange(height, nameof(height), 1);
			Helpers.Argument.ValidateRange(targetAspectRatio, nameof(targetAspectRatio), double.Epsilon);

			var inputAspectRatio = width * 1.0 / height;

			int outputHeight;
			int outputWidth;

			// 1) Decide which way to adjust - does the input need to become wider or narrower.
			// 2) Decrease the appropriate dimension of the input, giving the first output dimension.
			// 3) Calculate the other output dimension based on the target aspect ratio.
			// 4) Center the crop rectangle in the input.

			if (targetAspectRatio > inputAspectRatio)
			{
				// We need to make it wider - crop top and bottom.
				// We round the results to avoid needless cropping but avoid going out of bounds at all times.
				outputHeight = (int)Math.Min(height, Math.Round(width / targetAspectRatio, MidpointRounding.AwayFromZero));
				outputWidth = (int)Math.Min(width, Math.Round(outputHeight * targetAspectRatio, MidpointRounding.AwayFromZero));
			}
			else
			{
				// We need to make it narrower - crop left and right.
				// We round the results to avoid needless cropping but avoid going out of bounds at all times.
				outputWidth = (int)Math.Min(width, Math.Round(height * targetAspectRatio, MidpointRounding.AwayFromZero));
				outputHeight = (int)Math.Min(height, Math.Round(outputWidth / targetAspectRatio, MidpointRounding.AwayFromZero));
			}

			return new CropResult
			{
				Width = outputWidth,
				Height = outputHeight,
				XOffset = (width - outputWidth) / 2,
				YOffset = (height - outputHeight) / 2
			};
		}

		/// <summary>
		/// Extracts the key IDs from a protected media file. The contents of the input stream can be any supported
		/// type of presentation manifest or container file that contains key IDs.
		/// </summary>
		/// <remarks>
		/// Currently supports Smooth Streaming manifests as input.
		/// </remarks>
		/// <returns>
		/// The set of key IDs found or an empty set if the input was valid but contained no key IDs.
		/// No duplicates will be returned.
		/// </returns>
		public static ICollection<Guid> GetKeyIds(this HelpersContainerClasses.Media container, Stream stream)
		{
			Helpers.Argument.ValidateIsNotNull(stream, nameof(stream));

			var document = XDocument.Load(stream);
			var playReadyProtectionHeader = document.Root.Element("Protection")?.Elements("ProtectionHeader")?.Where(ph =>
			{
				var systemIdAttribute = ph.Attribute("SystemID");

				if (systemIdAttribute == null)
					return false;

				var systemId = Guid.Parse(systemIdAttribute.Value);

				return systemId == PlayReadyConstants.SystemId;
			}).FirstOrDefault();

			if (playReadyProtectionHeader == null)
				return new Guid[0];

			var playReadyHeader = Convert.FromBase64String(playReadyProtectionHeader.Value);
			var headerKeyId = Helpers.PlayReady.GetKeyIdFromPlayReadyHeader(playReadyHeader);

			return new[]
			{
				headerKeyId
			};
		}

		/// <summary>
		/// Creates a PSSH (Protection System Specific Header) box suitable for embedding into a media
		/// file that follows the ISO Base Media File Format specification.
		/// </summary>
		public static byte[] CreatePsshBox(this HelpersContainerClasses.Media container, Guid systemId, byte[] data)
		{
			Helpers.Argument.ValidateIsNotNull(data, nameof(data));

			// Size (32) BE
			// Type (32)
			// Version (8)
			// Flags (24)
			// SystemID (16*8) BE
			// DataSize (32) BE
			// Data (DataSize*8)

			using (var buffer = new MemoryStream())
			{
				using (var writer = new MultiEndianBinaryWriter(buffer, ByteOrder.BigEndian))
				{
					writer.Write(4 + 4 + 1 + 3 + 16 + 4 + data.Length);
					writer.Write(new[] { 'p', 's', 's', 'h' });
					writer.Write(0); // 0 flags, 0 version.
					writer.Write(systemId.ToBigEndianByteArray());
					writer.Write(data.Length);
					writer.Write(data);
				}

				return buffer.ToArray();
			}
		}
	}
}