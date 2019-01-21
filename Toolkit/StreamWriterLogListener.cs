namespace Axinom.Toolkit
{
    using System;
    using System.IO;

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

        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
                severity, timestamp, source, messageGenerator());

            _writer.WriteLine(message);
            _writer.WriteLine();
        }

        private readonly StreamWriter _writer;
    }
}