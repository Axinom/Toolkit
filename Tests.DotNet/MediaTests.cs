namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class MediaTests : TestClass
	{
		[Fact]
		public void GetKeyIds_WithSingleKeySmoothManifest_ReturnsExpectedKey()
		{
			using (var package = new EmbeddedPackage(typeof(MediaTests).GetTypeInfo().Assembly, "Tests.TestData", "SingleKeySmoothManifest.xml"))
			{
				using (var file = File.OpenRead(Path.Combine(package.Path, "SingleKeySmoothManifest.xml")))
				{
					var keyIds = Helpers.Media.GetKeyIds(file);

					Assert.Equal(1, keyIds.Count);
					Assert.Equal(new Guid("641c676f-432d-4e9e-89fa-51f4c06ff85e"), keyIds.Single());
				}
			}
		}

		[Theory]
		[InlineData(100, 100, 1d)]
		[InlineData(100, 100, 1.001d)]
		[InlineData(100, 100, 0.999d)]
		[InlineData(1, 100, 0.999d)]
		[InlineData(100, 1, 0.999d)]
		[InlineData(1, 100, 1.001d)]
		[InlineData(100, 1, 1.001d)]
		[InlineData(640, 480, 16d / 9d)]
		[InlineData(640, 360, 16d / 9d)]
		[InlineData(2680, 1080, 16d / 9d)]
		public void Crop_DoesNotGoOutOfBounds(int width, int height, double aspectRatio)
		{
			var crop = Helpers.Media.Crop(width, height, aspectRatio);

			Log.Default.Debug($"{width}x{height} @{aspectRatio} = {crop.Width}x{crop.Height}");

			Assert.True(crop.Width <= width);
			Assert.True(crop.Height <= height);
			Assert.True(crop.Width + crop.XOffset <= width);
			Assert.True(crop.Height + crop.YOffset <= height);
			Assert.True(crop.XOffset >= 0);
			Assert.True(crop.YOffset >= 0);
		}

		[Theory]
		[InlineData(100, 100, 1d, 100, 100)]
		[InlineData(100, 100, 1.001d, 100, 100)]
		[InlineData(100, 100, 0.999d, 100, 100)]
		[InlineData(1, 100, 0.999d, 1, 1)]
		[InlineData(100, 1, 0.999d, 1, 1)]
		[InlineData(1, 100, 1.001d, 1, 1)]
		[InlineData(100, 1, 1.001d, 1, 1)]
		[InlineData(300, 300, 2d, 300, 150)]
		[InlineData(300, 300, 0.5d, 150, 300)]
		[InlineData(640, 480, 16d / 9d, 640, 360)]
		[InlineData(640, 360, 16d / 9d, 640, 360)]
		[InlineData(2680, 1080, 16d / 9d, 1920, 1080)]
		public void Crop_ProvidesOptimalSize(int width, int height, double aspectRatio, int expectedWidth, int expectedHeight)
		{
			var crop = Helpers.Media.Crop(width, height, aspectRatio);

			Log.Default.Debug($"{width}x{height} @{aspectRatio} = {crop.Width}x{crop.Height}");

			Assert.Equal(expectedWidth, crop.Width);
			Assert.Equal(expectedHeight, crop.Height);
		}
	}
}