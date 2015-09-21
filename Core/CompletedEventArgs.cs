namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// An immutable general-purpose event arguments structure for "operation completed" events.
	/// Use the provided static methods for instantiation.
	/// </summary>
	public class CompletedEventArgs : CompletedEventArgs<object>
	{
		protected CompletedEventArgs(object result, Exception error, object correlationData)
			: base(result, error, correlationData)
		{
		}

		#region Static construction methods
		public new static CompletedEventArgs WithError(Exception error)
		{
			return new CompletedEventArgs(null, error, null);
		}

		public new static CompletedEventArgs WithError(Exception error, object correlationData)
		{
			return new CompletedEventArgs(null, error, correlationData);
		}

		public new static CompletedEventArgs WithResult(object result)
		{
			return new CompletedEventArgs(result, null, null);
		}

		public new static CompletedEventArgs WithResult(object result, object correlationDAta)
		{
			return new CompletedEventArgs(result, null, correlationDAta);
		}

		public new static CompletedEventArgs WithoutResult()
		{
			return new CompletedEventArgs(null, null, null);
		}

		public new static CompletedEventArgs WithoutResult(object correlationData)
		{
			return new CompletedEventArgs(null, null, correlationData);
		}
		#endregion
	}

	/// <summary>
	/// An immutable general-purpose event arguments structure for "operation completed" events.
	/// Use the provided static methods for instantiation.
	/// </summary>
	/// <typeparam name="TResult">Type of the operation result.</typeparam>
	public class CompletedEventArgs<TResult> : EventArgs
	{
		/// <summary>
		/// Gets the exception that happened during the processing of the operation.
		/// Null if the operation was a success.
		/// </summary>
		public Exception Error { get; private set; }

		/// <summary>
		/// Result of the operation.
		/// </summary>
		public TResult Result { get; private set; }

		/// <summary>
		/// Custom data for asynchronous request correlation.
		/// </summary>
		public object CorrelationData { get; private set; }

		protected CompletedEventArgs(TResult result, Exception error, object correlationData)
		{
			Error = error;
			Result = result;
			CorrelationData = correlationData;
		}

		#region Static construction methods
		public static CompletedEventArgs<TResult> WithError(Exception error)
		{
			return new CompletedEventArgs<TResult>(default(TResult), error, null);
		}

		public static CompletedEventArgs<TResult> WithError(Exception error, object correlationData)
		{
			return new CompletedEventArgs<TResult>(default(TResult), error, correlationData);
		}

		public static CompletedEventArgs<TResult> WithResult(TResult result)
		{
			return new CompletedEventArgs<TResult>(result, null, null);
		}

		public static CompletedEventArgs<TResult> WithResult(TResult result, object correlationDAta)
		{
			return new CompletedEventArgs<TResult>(result, null, correlationDAta);
		}

		public static CompletedEventArgs<TResult> WithoutResult()
		{
			return new CompletedEventArgs<TResult>(default(TResult), null, null);
		}

		public static CompletedEventArgs<TResult> WithoutResult(object correlationData)
		{
			return new CompletedEventArgs<TResult>(default(TResult), null, correlationData);
		}
		#endregion
	}

	/// <summary>
	/// An event delegate with <see cref="CompletedEventArgs"/> as the argument type.
	/// </summary>
	public delegate void CompletedEvent(object sender, CompletedEventArgs e);

	/// <summary>
	/// An event delegate with <see cref="CompletedEventArgs{TResult}"/> as the argument type.
	/// </summary>
	/// <typeparam name="TResult">Type of the operation's result value.</typeparam>
	public delegate void CompletedEvent<TResult>(object sender, CompletedEventArgs<TResult> e);
}