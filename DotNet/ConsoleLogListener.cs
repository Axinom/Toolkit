namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// A log listener that writes the output to the standard output and standard error streams.
	/// </summary>
	public sealed class ConsoleLogListener : ILogListener
	{
		public void OnWrite(LogEntry entry)
		{
			// We pad the severity string so that all entries are nicely aligned.
			var severityString = entry.Severity.ToString().PadRight(_maxSeverityStringLength);

			var message = string.Format("{0} {1:u} [{2}] {3}",
				severityString, entry.Timestamp, entry.Source, entry.Message);

			switch (entry.Severity)
			{
				case LogEntrySeverity.Error:
				case LogEntrySeverity.Wtf:
					Console.Error.WriteLine(message);
					break;
				case LogEntrySeverity.Warning:
					Console.Error.WriteLine(message);
					break;
				default:
					Console.Out.WriteLine(message);
					break;
			}
		}

		public void Dispose()
		{
		}

		private static readonly int _maxSeverityStringLength;

		static ConsoleLogListener()
		{
			_maxSeverityStringLength = Enum.GetNames(typeof(LogEntrySeverity)).Max(s => s.Length);
		}
	}
}