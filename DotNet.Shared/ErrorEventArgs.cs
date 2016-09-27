namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Generic event arguments class for error events, raised when an exception occurs.
	/// </summary>
	public class ErrorEventArgs : EventArgs
	{
		public Exception Error { get; protected set; }
		public object CorrelationData { get; protected set; }

		public ErrorEventArgs(Exception error, object correlationData)
		{
			Error = error;
			CorrelationData = correlationData;
		}

		public ErrorEventArgs(Exception error)
		{
			Error = error;
		}
	}
}