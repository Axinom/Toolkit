namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// A log listener that outputs the logging statements to a StreamWriter.
	/// </summary>
	public sealed class StreamWriterLogListener : ILogListener
	{
		public StreamWriterLogListener(StreamWriter writer)
		{
			Helpers.Argument.ValidateIsNotNull(writer, nameof(writer));

			_writer = writer;
		}

		public void Dispose()
		{
			_writer.Dispose();
		}

		public void OnWrite(LogEntry entry)
		{
			var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
				entry.Severity, entry.Timestamp, entry.Source, entry.Message);

			_writer.WriteLine(message);
			_writer.WriteLine();
		}

		private readonly StreamWriter _writer;
	}
}