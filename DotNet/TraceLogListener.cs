namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Writes log entries to trace output.
	/// </summary>
	public sealed class TraceLogListener : ILogListener
	{
		public void OnWrite(LogEntry entry)
		{
			var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
				entry.Severity, entry.Timestamp, entry.Source, entry.Message);

			switch (entry.Severity)
			{
				case LogEntrySeverity.Error:
				case LogEntrySeverity.Wtf:
					Trace.TraceError(message);
					break;
				case LogEntrySeverity.Warning:
					Trace.TraceWarning(message);
					break;
				case LogEntrySeverity.Info:
					Trace.TraceInformation(message);
					break;
				default:
					Trace.WriteLine(message);
					break;
			}
		}

		public void Dispose()
		{
		}
	}
}