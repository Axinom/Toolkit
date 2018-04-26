namespace Axinom.Toolkit
{
    using System;
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
		public CompositingStream(params CompositedStreamInfo[] children)
		{
			Helpers.Argument.ValidateIsNotNull(children, "children");

            if (children.Any(c => !c.Stream.CanSeek))
                throw new ArgumentException("All child streams must be seekable.");

            _children = children;

            _length = _children.Sum(c => c.Length);
            _canWrite = _children.All(c => c.Stream.CanWrite);

            // To be readable, all streams must have correct length.
            _canRead = _children.All(c => c.Stream.CanRead) && _children.All(c => c.Stream.Length == c.Length);
        }

        public CompositingStream(params Stream[] children)
		{
			Helpers.Argument.ValidateIsNotNull(children, "children");

			if (children.Any(c => !c.CanSeek))
				throw new ArgumentException("All child streams must be seekable.");

			_children = children.Select(c => new CompositedStreamInfo(c, c.Length)).ToArray();

            _length = _children.Sum(c => c.Length);
            _canWrite = _children.All(c => c.Stream.CanWrite);

            // To be readable, all streams must have correct length.
            _canRead = _children.All(c => c.Stream.CanRead) && _children.All(c => c.Stream.Length == c.Length);
        }

        private readonly CompositedStreamInfo[] _children;

        private readonly long _length;
        private readonly bool _canRead;
        private readonly bool _canWrite;

        #region Overrides of Stream
        public override void Flush()
		{
			foreach (var child in _children)
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

			if (target < 0 || target >= _length)
				throw new ArgumentOutOfRangeException("offset");

			var diff = _position - target;
			_position = target;

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
				foreach (var child in _children)
					child.Stream.Dispose();
			}

			base.Dispose(disposing);
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			Helpers.Argument.ValidateIsNotNull(buffer, "buffer");
			Helpers.Argument.ValidateRange(offset, "offset", min: 0, max: buffer.Length);
			Helpers.Argument.ValidateRange(count, "count", min: 0, max: buffer.Length - offset);

			if (!_canRead)
				throw new InvalidOperationException("At least one child stream is not readable or was not initialized at its full length.");

			int bytesRead = 0;

			while (count > 0 && _position < _length)
			{
				var stream = GetAndSeekActionableStream();

				var readSize = stream.Read(buffer, offset, count);

				offset += readSize;
				count -= readSize;
				bytesRead += readSize;
				_position += readSize;
			}

			return bytesRead;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Helpers.Argument.ValidateIsNotNull(buffer, "buffer");
			Helpers.Argument.ValidateRange(offset, "offset", min: 0, max: buffer.Length);
			Helpers.Argument.ValidateRange(count, "count", min: 0, max: buffer.Length - offset);

			if (!_canWrite)
				throw new InvalidOperationException("At least one child stream is not writable.");

			if (_position + count > _length)
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
				_position += writeSize;
			}
		}

        public override bool CanRead => _canRead;
        public override bool CanSeek => true;
        public override bool CanWrite => _canWrite;
        public override long Length => _length;

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

			var remaining = _length - _position;

			for (int i = _children.Length - 1; i >= 0; i--)
			{
				remaining -= _children[i].Length;

				if (remaining <= 0)
				{
					// This seeks to the right offset inside the stream.
                    if (remaining != 0)
					    _children[i].Stream.Position = -remaining;

					return _children[i].Stream;
				}
			}

			throw new Exception("Logic error - this point should be unreachable.");
		}

		private CompositedStreamInfo GetActionableStreamInfo()
		{
			// We work backwards from the end to find the best candidate.
			// This avoids ever seeking to the end of any stream at the beginning of an operation.

			var remaining = _length - _position;

			for (int i = _children.Length - 1; i >= 0; i--)
			{
				remaining -= _children[i].Length;

				if (remaining <= 0)
					return _children[i];
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