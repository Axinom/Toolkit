namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public sealed class FilesystemTests : BaseTestClass
    {
        [TestMethod]
        public void RemoveRoot_RemovesRootFromPath()
        {
            var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
            var root = MakeRootedPath("aa");

            var relativePath = Helpers.Filesystem.RemoveRoot(path, root);

            Assert.AreEqual(Path.Combine("bb", "cc"), relativePath);
        }

        [TestMethod]
        public void RemoveRoot_WithAndWithoutSlashAtEndOfRoot_GivesSameResult()
        {
            var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
            var root1 = MakeRootedPath(Path.Combine("aa")) + Path.DirectorySeparatorChar;
            var root2 = MakeRootedPath(Path.Combine("aa"));

            var relativePath1 = Helpers.Filesystem.RemoveRoot(path, root1);
            var relativePath2 = Helpers.Filesystem.RemoveRoot(path, root2);

            Assert.AreEqual(relativePath1, relativePath2);
        }

        [TestMethod]
        public void RemoveRoot_WithInvalidRoot_ThrowsException()
        {
            var path = MakeRootedPath(Path.Combine("aa", "bb", "cc"));
            var root = MakeRootedPath("dd");

            Assert.ThrowsException<ArgumentException>(() => Helpers.Filesystem.RemoveRoot(path, root));
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