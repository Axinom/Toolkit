namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Implements a thread-safe log listener that accepts log entries that come from various log sources.
	/// The log listener will be disposed of automatically when it is unregistered or when the appplication exits.
	/// </summary>
	public interface ILogListener : IDisposable
	{
		/// <summary>
		/// Called when a log entry is written into the log.
		/// </summary>
		void OnWrite(LogEntry entry);
	}
}