namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class MediaTests
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
	}
}