namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class FilesystemTests
	{
		[Fact]
		public void RemoveRoot_RemovesRootFromPath()
		{
			var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
			var root = MakeRootedPath("aa");

			var relativePath = Helpers.Filesystem.RemoveRoot(path, root);

			Assert.Equal(Path.Combine("bb", "cc"), relativePath);
		}

		[Fact]
		public void RemoveRoot_WithAndWithoutSlashAtEndOfRoot_GivesSameResult()
		{
			var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
			var root1 = MakeRootedPath(Path.Combine("aa")) + Path.DirectorySeparatorChar;
			var root2 = MakeRootedPath(Path.Combine("aa"));

			var relativePath1 = Helpers.Filesystem.RemoveRoot(path, root1);
			var relativePath2 = Helpers.Filesystem.RemoveRoot(path, root2);

			Assert.Equal(relativePath1, relativePath2);
		}

		[Fact]
		public void RemoveRoot_WithInvalidRoot_ThrowsException()
		{
			var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
			var root = MakeRootedPath("dd");

			Assert.Throws<ArgumentException>(() => Helpers.Filesystem.RemoveRoot(path, root));
		}

		private static string MakeRootedPath(string path)
		{
			path = path.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

			if (Path.IsPathRooted(path))
				return path;

			if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
				return Path.DirectorySeparatorChar + path;
			else
				return "C:\\" + path;
		}
	}
}