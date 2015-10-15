namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class TemporaryDirectoryTests : TestClass
	{
		[Fact]
		public void TemporaryFolderIsCreatedAndRemoved()
		{
			string temporaryDirectory;

			using (var folder = new TemporaryDirectory())
			{
				temporaryDirectory = folder.Path;
				Assert.True(Directory.Exists(folder.Path));
			}

			Assert.False(Directory.Exists(temporaryDirectory));
		}

		[Fact]
		public void Initializer_WithCustomPrefix_CreatesDirectoryWithCustomPrefix()
		{
			const string prefix = "ez5b8 y5ekht ";

			using (var folder = new TemporaryDirectory(prefix))
			{
				Assert.StartsWith(prefix, Path.GetFileName(folder.Path));
			}
		}
	}
}