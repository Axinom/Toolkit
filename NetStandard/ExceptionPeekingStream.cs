namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Allows you to peek at any errors that occur while reading from or writing to the stream.
	/// This allows you to take a look at any exceptions that occur if the stream is being used
	/// by something not under your control (e.g. the WCF streaming system).
	/// </summary>
	/// <remarks>
	/// Exceptions are intercepted on the following calls:
	/// * Read()
	/// * Write()
	/// * Seek()
	/// * SetLength()
	/// * Flush()
	/// * Position setter
	/// </remarks>
	public sealed class ExceptionPeekingStream : Stream
	{
		/// <summary>
		/// Called when an exception occurs during a call into the stream.
		/// </summary>
		public event EventHandler<ErrorEventArgs> ExceptionOccurred = delegate { };

		private readonly Stream _wrapped;

		public ExceptionPeekingStream(Stream wrapped)
		{
			Helpers.Argument.ValidateIsNotNull(wrapped, "wrapped");

			_wrapped = wrapped;
		}

		private void ExecuteAndObserve(Action wrappedAction)
		{
			try
			{
				wrappedAction();
			}
			catch (Exception ex)
			{
				ExceptionOccurred(this, new ErrorEventArgs(ex));
				throw;
			}
		}

		private T ExecuteAndObserve<T>(Func<T> wrappedFunction)
		{
			try
			{
				return wrappedFunction();
			}
			catch (Exception ex)
			{
				ExceptionOccurred(this, new ErrorEventArgs(ex));
				throw;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			ExecuteAndObserve(() => _wrapped.Write(buffer, offset, count));
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return ExecuteAndObserve(() => _wrapped.Read(buffer, offset, count));
		}

		public override bool CanTimeout
		{
			[DebuggerStepThrough] get { return _wrapped.CanTimeout; }
		}

		public override void Flush()
		{
			ExecuteAndObserve(() => _wrapped.Flush());
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return ExecuteAndObserve(() => _wrapped.Seek(offset, origin));
		}

		public override void SetLength(long value)
		{
			ExecuteAndObserve(() => _wrapped.SetLength(value));
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

		public override long Position
		{
			[DebuggerStepThrough] get { return _wrapped.Position; }
			set { ExecuteAndObserve(() => _wrapped.Position = value); }
		}
	}
}