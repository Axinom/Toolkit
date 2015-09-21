namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Represents one child stream in a compositing stream.
	/// </summary>
	public sealed class CompositedStreamInfo
	{
		public Stream Stream { get; private set; }

		/// <summary>
		/// The nominal length of the stream. The stream itself does not have to be this length.
		/// For example, if it is a write stream, it may be created piece-by-piece as it is written.
		/// It would be wasteful to allocate the entire stream at once.
		/// </summary>
		public long Length { get; private set; }

		public CompositedStreamInfo(Stream stream, long length)
		{
			Helpers.Argument.ValidateIsNotNull(stream, "stream");
			Helpers.Argument.ValidateRange(length, "length", min: 0);

			Stream = stream;
			Length = length;
		}

		public CompositedStreamInfo(Stream stream)
		{
			Helpers.Argument.ValidateIsNotNull(stream, "stream");

			Stream = stream;
			Length = stream.Length;
		}
	}
}