namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class EmbeddedPackageTests : TestClass
	{
		private static readonly Assembly _testsAssembly;

		static EmbeddedPackageTests()
		{
			_testsAssembly = typeof(EmbeddedPackageTests).GetTypeInfo().Assembly;
		}

		[Fact]
		public void Dispose_RemovesFilesystemObjects()
		{
			string packagePath;

			using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll"))
			{
				packagePath = package.Path;

				Assert.True(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
			}

			Assert.False(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
			Assert.False(Directory.Exists(packagePath));
		}

		[Fact]
		public void Initializer_ExtractsSingleFile()
		{
			using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll"))
			{
				Assert.True(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
			}
		}

		[Fact]
		public void Initializer_ExtractsMultipleFiles()
		{
			using (var package = new EmbeddedPackage(_testsAssembly, "Tests.TestData", "MediaInfo.dll", "Gangster.xml"))
			{
				Assert.True(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
				Assert.True(File.Exists(Path.Combine(package.Path, "gangster.xml")));
			}
		}
	}
}