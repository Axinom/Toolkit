namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public class CompositingStreamTests
	{
		[Test]
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

		[Test]
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

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void ChildrenAreClosed()
		{
			var first = new MemoryStream(new[] { (byte)1 });
			var second = new MemoryStream(new[] { (byte)2 });

			var composite = new CompositingStream();
			composite.Children.Add(new CompositedStreamInfo(first, first.Length));
			composite.Children.Add(new CompositedStreamInfo(second));

			composite.Dispose();

			second.Seek(0, SeekOrigin.Begin);
		}

		[Test]
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

		[Test]
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