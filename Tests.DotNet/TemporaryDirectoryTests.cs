namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class TemporaryDirectoryTests
	{
		[Test]
		public void TemporaryFolderIsCreatedAndRemoved()
		{
			string temporaryDirectory;

			using (var folder = new TemporaryDirectory())
			{
				temporaryDirectory = folder.Path;
				Assert.IsTrue(Directory.Exists(folder.Path));
			}

			Assert.IsFalse(Directory.Exists(temporaryDirectory));
		}

		[Test]
		public void Initializer_WithCustomPrefix_CreatesDirectoryWithCustomPrefix()
		{
			const string prefix = "ez5b8 y5ekht ";

			using (var folder = new TemporaryDirectory(prefix))
			{
				StringAssert.StartsWith(prefix, Path.GetFileName(folder.Path));
			}
		}
	}
}