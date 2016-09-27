namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Wraps a Stream and exposes any Position updates as an event.
	/// </summary>
	public class PositionReportingStream : Stream
	{
		/// <summary>
		/// Raised when the stream position changes.
		/// </summary>
		public event EventHandler PositionChanged = delegate { };

		private readonly Stream _wrapped;

		public PositionReportingStream(Stream wrapped)
		{
			Helpers.Argument.ValidateIsNotNull(wrapped, "wrapped");

			_wrapped = wrapped;
		}

		public override long Position
		{
			[DebuggerStepThrough] get { return _wrapped.Position; }
			set
			{
				var oldPosition = _wrapped.Position;

				_wrapped.Position = value;

				if (oldPosition != _wrapped.Position)
					PositionChanged(this, EventArgs.Empty);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var oldPosition = _wrapped.Position;

			var result = _wrapped.Read(buffer, offset, count);

			if (oldPosition != _wrapped.Position)
				PositionChanged(this, EventArgs.Empty);

			return result;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			var oldPosition = _wrapped.Position;

			var result = _wrapped.Seek(offset, origin);

			if (oldPosition != _wrapped.Position)
				PositionChanged(this, EventArgs.Empty);

			return result;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			var oldPosition = _wrapped.Position;

			_wrapped.Write(buffer, offset, count);

			if (oldPosition != _wrapped.Position)
				PositionChanged(this, EventArgs.Empty);
		}

		public override void SetLength(long value)
		{
			var oldPosition = _wrapped.Position;

			_wrapped.SetLength(value);

			if (oldPosition != _wrapped.Position)
				PositionChanged(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_wrapped.Dispose();
			}

			base.Dispose(disposing);
		}

		public override bool CanTimeout
		{
			[DebuggerStepThrough] get { return _wrapped.CanTimeout; }
		}

		[DebuggerStepThrough]
		public override void Flush()
		{
			_wrapped.Flush();
		}

		public override bool CanRead
		{
			[DebuggerStepThrough] get { return _wrapped.CanRead; }
		}

		public override bool CanSeek
		{
			[DebuggerStepThrough] get { return _wrapped.CanSeek; }
		}

		public override bool CanWrite
		{
			[DebuggerStepThrough] get { return _wrapped.CanWrite; }
		}

		public override long Length
		{
			[DebuggerStepThrough] get { return _wrapped.Length; }
		}
	}
}