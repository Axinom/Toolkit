namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public sealed class CompositingStreamTests : BaseTestClass
    {
        [TestMethod]
        public void BasicWriteTest()
        {
            var first = new MemoryStream(new byte[1]);
            var second = new MemoryStream(new byte[1]);

            var composite = new CompositingStream(first, second);

            byte[] data = new[] { (byte)1, (byte)2 };
            composite.Write(data, 0, 2);

            Assert.AreEqual(1, first.ToArray()[0]);
            Assert.AreEqual(2, second.ToArray()[0]);
        }

        [TestMethod]
        public void BasicReadTest()
        {
            var first = new MemoryStream(new[] { (byte)1 });
            var second = new MemoryStream(new[] { (byte)2 });

            var composite = new CompositingStream(first, second);

            byte[] buffer = new byte[2];
            var readBytes = composite.Read(buffer, 0, 2);

            Assert.AreEqual(2, readBytes);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
        }

        [TestMethod]
        public void ChildrenAreClosed()
        {
            var first = new MemoryStream(new[] { (byte)1 });
            var second = new MemoryStream(new[] { (byte)2 });

            var composite = new CompositingStream(new[]
            {
                new CompositedStreamInfo(first, first.Length),
                new CompositedStreamInfo(second)
            });

            composite.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => second.Seek(0, SeekOrigin.Begin));
        }

        [TestMethod]
        public void EmptyStreamIgnoredOnWrite()
        {
            var first = new MemoryStream(new byte[1]);
            var empty = new MemoryStream(new byte[0]);
            var second = new MemoryStream(new byte[1]);

            var composite = new CompositingStream(first, empty, second);

            byte[] data = new[] { (byte)1, (byte)2 };
            composite.Write(data, 0, 2);

            Assert.AreEqual(1, first.ToArray()[0]);
            Assert.AreEqual(2, second.ToArray()[0]);
        }

        [TestMethod]
        public void EmptyStreamIgnoredOnRead()
        {
            var first = new MemoryStream(new[] { (byte)1 });
            var empty = new MemoryStream(new byte[0]);
            var second = new MemoryStream(new[] { (byte)2 });

            var composite = new CompositingStream(first, empty, second);

            byte[] buffer = new byte[2];
            var readBytes = composite.Read(buffer, 0, 2);

            Assert.AreEqual(2, readBytes);
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(2, buffer[1]);
        }
    }
}