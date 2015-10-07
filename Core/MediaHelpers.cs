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

			if (targetAspectRatio > inputAspectRatio)
			{
				// We need to make it wider - crop top and bottom.
				var targetHeight = (int)(width / targetAspectRatio);
				var cropPixels = height - targetHeight;

				return new CropResult
				{
					Width = width,
					Height = targetHeight,
					XOffset = 0,
					YOffset = cropPixels / 2
				};
			}
			else
			{
				// We need to make it narrower - crop left and right.
				var targetWidth = (int)(height * targetAspectRatio);
				var cropPixels = width - targetWidth;

				return new CropResult
				{
					Width = targetWidth,
					Height = height,
					XOffset = cropPixels / 2,
					YOffset = 0
				};
			}
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
	}
}