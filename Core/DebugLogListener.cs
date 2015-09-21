// We always want debug output enabled for this file, otherwise this class is useless.
#define DEBUG

namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Writes log entries to debugger output.
	/// </summary>
	public sealed class DebugLogListener : ILogListener
	{
		public void OnWrite(LogEntry entry)
		{
			var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
				entry.Severity, entry.Timestamp, entry.Source, entry.Message);

			Debug.WriteLine(message);
		}

		public void Dispose()
		{
		}
	}
}