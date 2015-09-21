namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

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
	}
}