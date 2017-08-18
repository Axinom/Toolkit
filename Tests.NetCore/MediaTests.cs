namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class MediaTests : TestClass
    {
        [TestMethod]
        public void GetKeyIds_WithSingleKeySmoothManifest_ReturnsExpectedKey()
        {
            foreach (var testDataRow in GetKeyIdTestData())
            {
                var filename = (string)testDataRow[0];
                Guid[] expectedKeyIds = (Guid[])testDataRow[1];

                using (var package = new EmbeddedPackage(typeof(MediaTests).GetTypeInfo().Assembly, "Tests.TestData", filename))
                {
                    using (var file = File.OpenRead(Path.Combine(package.Path, filename)))
                    {
                        var keyIds = Helpers.Media.GetKeyIds(file);

                        Assert.AreEqual(expectedKeyIds.Length, keyIds.Count);
                        CollectionAssert.AreEqual(expectedKeyIds.OrderBy(k => k).ToArray(), keyIds.OrderBy(k => k).ToArray());
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetKeyIdTestData()
        {
            yield return new object[] { "MultiPeriod_Manifest.mpd", new Guid[0] };
            yield return new object[] { "SingleKeySmoothManifest.xml", new[] { new Guid("641c676f-432d-4e9e-89fa-51f4c06ff85e") } };
            yield return new object[] { "Manifest_MultiDRM.mpd", new[] { new Guid("6e5a1d26-2757-47d7-8046-eaa5d1d34b5a") } };
            yield return new object[] { "Manifest_MultiKey.mpd", new[] { new Guid("1530d3a0-6904-446a-91a1-33a115aa8c41"), new Guid("c83eb639-e664-43f8-ae98-4039b0c13b2d"), new Guid("3d8cc762-27ac-400f-989f-8ab5dc7d7775"), new Guid("bd8dad58-032d-4c25-89fa-c7b710e82ac2") } };
            yield return new object[] { "Manifest_MultiPeriod_MultiKey.mpd", new[] { new Guid("53be7757-7288-4b6b-b20a-f05b64a4ef79"), new Guid("0ed821a8-80ed-40ac-a804-927c9fdadbe9"), new Guid("e47d78ca-94dc-45fb-9e3d-2a773aef74b2"), new Guid("32a141e9-23ab-44ff-a6c7-5349c89451cf"), new Guid("8d091966-44b5-4cf8-8a45-ed12fdb18d35") } };
        }

        [DataTestMethod]
        [DataRow(100, 100, 1d)]
        [DataRow(100, 100, 1.001d)]
        [DataRow(100, 100, 0.999d)]
        [DataRow(1, 100, 0.999d)]
        [DataRow(100, 1, 0.999d)]
        [DataRow(1, 100, 1.001d)]
        [DataRow(100, 1, 1.001d)]
        [DataRow(640, 480, 16d / 9d)]
        [DataRow(640, 360, 16d / 9d)]
        [DataRow(2680, 1080, 16d / 9d)]
        public void Crop_DoesNotGoOutOfBounds(int width, int height, double aspectRatio)
        {
            var crop = Helpers.Media.Crop(width, height, aspectRatio);

            Log.Default.Debug($"{width}x{height} @{aspectRatio} = {crop.Width}x{crop.Height}");

            Assert.IsTrue(crop.Width <= width);
            Assert.IsTrue(crop.Height <= height);
            Assert.IsTrue(crop.Width + crop.XOffset <= width);
            Assert.IsTrue(crop.Height + crop.YOffset <= height);
            Assert.IsTrue(crop.XOffset >= 0);
            Assert.IsTrue(crop.YOffset >= 0);
        }

        [DataTestMethod]
        [DataRow(100, 100, 1d, 100, 100)]
        [DataRow(100, 100, 1.001d, 100, 100)]
        [DataRow(100, 100, 0.999d, 100, 100)]
        [DataRow(1, 100, 0.999d, 1, 1)]
        [DataRow(100, 1, 0.999d, 1, 1)]
        [DataRow(1, 100, 1.001d, 1, 1)]
        [DataRow(100, 1, 1.001d, 1, 1)]
        [DataRow(300, 300, 2d, 300, 150)]
        [DataRow(300, 300, 0.5d, 150, 300)]
        [DataRow(640, 480, 16d / 9d, 640, 360)]
        [DataRow(640, 360, 16d / 9d, 640, 360)]
        [DataRow(2680, 1080, 16d / 9d, 1920, 1080)]
        public void Crop_ProvidesOptimalSize(int width, int height, double aspectRatio, int expectedWidth, int expectedHeight)
        {
            var crop = Helpers.Media.Crop(width, height, aspectRatio);

            Log.Default.Debug($"{width}x{height} @{aspectRatio} = {crop.Width}x{crop.Height}");

            Assert.AreEqual(expectedWidth, crop.Width);
            Assert.AreEqual(expectedHeight, crop.Height);
        }
    }
}