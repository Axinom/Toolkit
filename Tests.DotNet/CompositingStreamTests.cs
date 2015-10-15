﻿namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class CompositingStreamTests : TestClass
	{
		[Fact]
		public void BasicWriteTest()
		{
			var first = new MemoryStream(new byte[1]);
			var second = new MemoryStream(new byte[1]);

			var composite = new CompositingStream(first, second);

			byte[] data = new[] { (byte)1, (byte)2 };
			composite.Write(data, 0, 2);

			Assert.Equal(1, first.ToArray()[0]);
			Assert.Equal(2, second.ToArray()[0]);
		}

		[Fact]
		public void BasicReadTest()
		{
			var first = new MemoryStream(new[] { (byte)1 });
			var second = new MemoryStream(new[] { (byte)2 });

			var composite = new CompositingStream(first, second);

			byte[] buffer = new byte[2];
			var readBytes = composite.Read(buffer, 0, 2);

			Assert.Equal(2, readBytes);
			Assert.Equal(1, buffer[0]);
			Assert.Equal(2, buffer[1]);
		}

		[Fact]
		public void ChildrenAreClosed()
		{
			var first = new MemoryStream(new[] { (byte)1 });
			var second = new MemoryStream(new[] { (byte)2 });

			var composite = new CompositingStream();
			composite.Children.Add(new CompositedStreamInfo(first, first.Length));
			composite.Children.Add(new CompositedStreamInfo(second));

			composite.Dispose();

			Assert.Throws<ObjectDisposedException>(() => second.Seek(0, SeekOrigin.Begin));
		}

		[Fact]
		public void EmptyStreamIgnoredOnWrite()
		{
			var first = new MemoryStream(new byte[1]);
			var empty = new MemoryStream(new byte[0]);
			var second = new MemoryStream(new byte[1]);

			var composite = new CompositingStream(first, empty, second);

			byte[] data = new[] { (byte)1, (byte)2 };
			composite.Write(data, 0, 2);

			Assert.Equal(1, first.ToArray()[0]);
			Assert.Equal(2, second.ToArray()[0]);
		}

		[Fact]
		public void EmptyStreamIgnoredOnRead()
		{
			var first = new MemoryStream(new[] { (byte)1 });
			var empty = new MemoryStream(new byte[0]);
			var second = new MemoryStream(new[] { (byte)2 });

			var composite = new CompositingStream(first, empty, second);

			byte[] buffer = new byte[2];
			var readBytes = composite.Read(buffer, 0, 2);

			Assert.Equal(2, readBytes);
			Assert.Equal(1, buffer[0]);
			Assert.Equal(2, buffer[1]);
		}
	}
}