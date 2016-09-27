namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Represents a log source - the place where log entries enter the logging system.
	/// Thread-safe - this object may be used from any thread.
	/// </summary>
	/// <remarks>
	/// You will probably want to associate a log source with every specific component of your application.
	/// You can accomplish this by creating hierarchical child log sources with the name of every component.
	/// </remarks>
	public abstract class LogSource
	{
		/// <summary>
		/// Creates a log source that is a child of the current one. Log entries from the child log source will be
		/// marked with the name of the child log source appended to the name of the current log source.
		/// </summary>
		public LogSource CreateChildSource(string name)
		{
			Helpers.Argument.ValidateIsNotNull(name, "name");

			return new ChildLogSource(name, this);
		}

		public void Debug(string message, params object[] args)
		{
			Write(LogEntrySeverity.Debug, null, message, args);
		}

		public void Info(string message, params object[] args)
		{
			Write(LogEntrySeverity.Info, null, message, args);
		}

		public void Warning(string message, params object[] args)
		{
			Write(LogEntrySeverity.Warning, null, message, args);
		}

		public void Error(string message, params object[] args)
		{
			Write(LogEntrySeverity.Error, null, message, args);
		}

		public void Wtf(string message, params object[] args)
		{
			Write(LogEntrySeverity.Wtf, null, message, args);
		}

		/// <summary>
		/// Called when this log source or a child log source has a log entry to write out into the log.
		/// </summary>
		internal abstract void Write(LogEntrySeverity severity, string originalSource, string message, params object[] args);
	}
}