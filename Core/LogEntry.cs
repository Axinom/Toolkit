namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public sealed class LogEntry
	{
		/// <summary>
		/// Log entry severity, allows filtering and prioritization.
		/// </summary>
		public LogEntrySeverity Severity { get; private set; }

		/// <summary>
		/// The log message itself.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// When the log entry was created (UTC).
		/// </summary>
		public DateTimeOffset Timestamp { get; private set; }

		/// <summary>
		/// String indicating the source of the log entry. May be null.
		/// </summary>
		public string Source { get; private set; }

		internal LogEntry(LogEntrySeverity severity, string message, string source)
		{
			Helpers.Argument.ValidateEnum(severity, "level");
			Helpers.Argument.ValidateIsNotNull(message, "message");

			Severity = severity;
			Message = message;
			Source = source;

			Timestamp = DateTimeOffset.UtcNow;
		}
	}
}