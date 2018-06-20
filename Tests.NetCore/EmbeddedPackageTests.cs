namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Reflection;

    [TestClass]
    public sealed class EmbeddedPackageTests : BaseTestClass
    {
        private static readonly Assembly _testsAssembly;

        static EmbeddedPackageTests()
        {
            _testsAssembly = typeof(EmbeddedPackageTests).GetTypeInfo().Assembly;
        }

        [TestMethod]
        public void Dispose_RemovesFilesystemObjects()
        {
            string packagePath;

            using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll"))
            {
                packagePath = package.Path;

                Assert.IsTrue(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
            }

            Assert.IsFalse(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
            Assert.IsFalse(Directory.Exists(packagePath));
        }

        [TestMethod]
        public void Initializer_ExtractsSingleFile()
        {
            using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll"))
            {
                Assert.IsTrue(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
            }
        }

        [TestMethod]
        public void Initializer_ExtractsMultipleFiles()
        {
            using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll", "Gangster.xml"))
            {
                Assert.IsTrue(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
                Assert.IsTrue(File.Exists(Path.Combine(package.Path, "gangster.xml")));
            }
        }
    }
}