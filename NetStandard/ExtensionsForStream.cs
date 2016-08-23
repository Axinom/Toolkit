namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static class ExtensionsForStream
	{
		/// <summary>
		/// Reads a number of bytes from the stream and verifies that the desired number of bytes was actually read.
		/// </summary>
		public static void ReadAndVerify(this Stream instance, byte[] buffer, int offset, int count)
		{
			int byteCount = instance.Read(buffer, offset, count);

			if (byteCount != count)
				throw new IOException(string.Format("Stream did not return expected number of bytes on Read(). Expected {0} but got {1}.", count, byteCount));
		}

		/// <summary>
		/// Reads a single byte from the stream and throws an EndOfStreamException if there is no more data in the stream.
		/// </summary>
		public static byte ReadByteAndVerify(this Stream instance)
		{
			int readByte = instance.ReadByte();

			if (readByte == -1)
				throw new EndOfStreamException();

			return (byte)readByte;
		}

		/// <summary>
		/// Copies bytes from one stream to another, using a generally performant buffered copy.
		/// </summary>
		public static void CopyBytes(this Stream instance, Stream to, long length)
		{
			Helpers.Argument.ValidateIsNotNull(instance, "instance");
			Helpers.Argument.ValidateIsNotNull(to, "to");

			long bytesAvailable = instance.Length - instance.Position;

			if (bytesAvailable < length)
				throw new ArgumentException("There are not enough bytes in the stream.", "length");

			byte[] buffer = new byte[Math.Min(length, 16 * 1024)];

			long remaining = length;
			while (remaining > 0)
			{
				int bytesRead = instance.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
				if (bytesRead == 0)
					throw new ArgumentException("The stream did not provide data when it was asked to.", "instance");

				to.Write(buffer, 0, bytesRead);
				remaining -= bytesRead;
			}
		}

		private const int FakeSeekBufferSize = 8 * 1024;

		/// <summary>
		/// Seeks a stream forward or reads the equivalent number of bytes if the stream does not support seeking.
		/// </summary>
		public static void SeekOrReadForward(this Stream stream, long bytes)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (bytes < 0)
				throw new ArgumentOutOfRangeException("bytes", "Number of bytes must not be negative.");

			if (stream.CanSeek)
			{
				stream.Seek(bytes, SeekOrigin.Current);
			}
			else
			{
				byte[] fakeSeekBuffer = new byte[FakeSeekBufferSize];

				while (bytes > 0)
				{
					int chunkSize = (int)Math.Min(bytes, fakeSeekBuffer.Length);

					int realChunkSize = stream.Read(fakeSeekBuffer, 0, chunkSize);
					if (realChunkSize == 0)
						throw new EndOfStreamException();

					bytes -= realChunkSize;
				}
			}
		}
	}
}