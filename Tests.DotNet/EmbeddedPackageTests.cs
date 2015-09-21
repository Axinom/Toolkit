namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class EmbeddedPackageTests
	{
		[Test]
		public void Dispose_RemovesFilesystemObjects()
		{
			string packagePath;

			using (var package = new EmbeddedPackage(Assembly.GetExecutingAssembly(), "Tests.DotNet.TestData", "MediaInfo.dll"))
			{
				packagePath = package.Path;

				Assert.IsTrue(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
			}

			Assert.IsFalse(File.Exists(Path.Combine(packagePath, "MediaInfo.dll")));
			Assert.IsFalse(Directory.Exists(packagePath));
		}

		[Test]
		public void Initializer_ExtractsSingleFile()
		{
			using (var package = new EmbeddedPackage(Assembly.GetExecutingAssembly(), "Tests.DotNet.TestData", "MediaInfo.dll"))
			{
				Assert.IsTrue(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
			}
		}

		[Test]
		public void Initializer_ExtractsMultipleFiles()
		{
			using (var package = new EmbeddedPackage(Assembly.GetExecutingAssembly(), "Tests.DotNet.TestData", "MediaInfo.dll", "Gangster.xml"))
			{
				Assert.IsTrue(File.Exists(Path.Combine(package.Path, "MediaInfo.dll")));
				Assert.IsTrue(File.Exists(Path.Combine(package.Path, "gangster.xml")));
			}
		}
	}
}