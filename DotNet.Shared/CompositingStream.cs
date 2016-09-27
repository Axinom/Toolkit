namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// A stream that reads from or writes to any number of fixed-length streams.
	/// </summary>
	/// <remarks>
	/// For read operations, the child streams must be full-length (e.g. Stream.Length == CompositedStreamInfo.Length).
	/// </remarks>
	public class CompositingStream : Stream
	{
		#region List<CompositedStreamInfo> Children
		public List<CompositedStreamInfo> Children
		{
			get { return _children; }
			set { _children = value ?? new List<CompositedStreamInfo>(); }
		}

		private List<CompositedStreamInfo> _children = new List<CompositedStreamInfo>();
		#endregion

		#region Initialization
		public CompositingStream()
		{
		}

		public CompositingStream(params CompositedStreamInfo[] children)
		{
			Helpers.Argument.ValidateIsNotNull(children, "children");

			Children = children.ToList();
		}

		public CompositingStream(params Stream[] children)
		{
			Helpers.Argument.ValidateIsNotNull(children, "children");

			if (children.Any(c => !c.CanSeek))
				throw new ArgumentException("All child streams must be seekable.");

			Children = children.Select(c => new CompositedStreamInfo(c, c.Length)).ToList();
		}
		#endregion

		#region Overrides of Stream
		public override void Flush()
		{
			foreach (var child in Children)
				child.Stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long target;

			switch (origin)
			{
				case SeekOrigin.Begin:
					target = offset;
					break;
				case SeekOrigin.Current:
					target = Position + offset;
					break;
				case SeekOrigin.End:
					target = Length + offset;
					break;
				default:
					throw new NotSupportedException("Unexpected seek origin: " + origin);
			}

			if (target < 0 || target >= Length)
				throw new ArgumentOutOfRangeException("offset");

			var diff = Position - target;
			Position = target;

			return diff;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("CompositingStream is always a fixed-length stream - cannot set length.");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var child in Children)
					child.Stream.Dispose();
			}

			base.Dispose(disposing);
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			Helpers.Argument.ValidateIsNotNull(buffer, "buffer");
			Helpers.Argument.ValidateRange(offset, "offset", min: 0, max: buffer.Length);
			Helpers.Argument.ValidateRange(count, "count", min: 0, max: buffer.Length - offset);

			if (!CanSeek)
				throw new InvalidOperationException("At least one child stream is not seekable.");

			if (!CanRead)
				throw new InvalidOperationException("At least one child stream is not readable or not full-length.");

			int bytesRead = 0;

			while (count > 0 && Position < Length)
			{
				var stream = GetAndSeekActionableStream();

				var readSize = stream.Read(buffer, offset, count);

				offset += readSize;
				count -= readSize;
				bytesRead += readSize;
				Position += readSize;
			}

			return bytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Helpers.Argument.ValidateIsNotNull(buffer, "buffer");
			Helpers.Argument.ValidateRange(offset, "offset", min: 0, max: buffer.Length);
			Helpers.Argument.ValidateRange(count, "count", min: 0, max: buffer.Length - offset);

			if (!CanSeek)
				throw new InvalidOperationException("At least one child stream is not seekable.");

			if (!CanWrite)
				throw new InvalidOperationException("At least one child stream is not writable.");

			if (Position + count > Length)
				throw new ArgumentException("Data to be written does not fit in the fixed-length stream.");

			while (count > 0)
			{
				var stream = GetAndSeekActionableStream();
				var streamLength = GetActionableStreamNominalLength();

				var roomLeftInStream = streamLength - stream.Position;
				var writeSize = (int)Math.Min(count, roomLeftInStream);

				stream.Write(buffer, offset, writeSize);

				offset += writeSize;
				count -= writeSize;
				Position += writeSize;
			}
		}

		public override bool CanRead
		{
			get
			{
				return Children.All(c => c.Stream.CanRead)
				       && Children.All(c => c.Stream.Length == c.Length);
			}
		}

		public override bool CanSeek
		{
			get { return Children.All(c => c.Stream.CanSeek); }
		}

		public override bool CanWrite
		{
			get { return Children.All(c => c.Stream.CanWrite); }
		}

		public override long Length
		{
			get { return Children.Sum(c => c.Length); }
		}

		public override long Position
		{
			get { return _position; }
			set
			{
				if (value < 0 || value > Length)
					throw new ArgumentOutOfRangeException("value");

				_position = value;
			}
		}

		private long _position;
		#endregion

		#region Implementation details
		/// <summary>
		/// Gets the stream that is to be used at the current position and seeks it to the right offset, if required.
		/// </summary>
		private Stream GetAndSeekActionableStream()
		{
			// We work backwards from the end to find the best candidate.
			// This avoids ever seeking to the end of any stream at the beginning of an operation.

			var remaining = Length - Position;

			for (int i = Children.Count - 1; i >= 0; i--)
			{
				remaining -= Children[i].Length;

				if (remaining <= 0)
				{
					// This seeks to the right offset inside the stream.
					Children[i].Stream.Position = -remaining;

					return Children[i].Stream;
				}
			}

			throw new Exception("Logic error - this point should be unreachable.");
		}

		private CompositedStreamInfo GetActionableStreamInfo()
		{
			// We work backwards from the end to find the best candidate.
			// This avoids ever seeking to the end of any stream at the beginning of an operation.

			var remaining = Length - Position;

			for (int i = Children.Count - 1; i >= 0; i--)
			{
				remaining -= Children[i].Length;

				if (remaining <= 0)
					return Children[i];
			}

			throw new Exception("Logic error - this point should be unreachable.");
		}

		private long GetActionableStreamNominalLength()
		{
			return GetActionableStreamInfo().Length;
		}
		#endregion
	}
}