namespace Axinom.Toolkit
{
    using System;
    using System.Linq;

    /// <summary>
    /// A log listener that writes the output to the standard output and standard error streams.
    /// </summary>
    public sealed class ConsoleLogListener : ILogListener
    {
        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            // We pad the severity string so that all entries are nicely aligned.
            var severityString = severity.ToString().PadRight(_maxSeverityStringLength);

            var message = string.Format("{0} {1:u} [{2}] {3}",
                severityString, timestamp, source, messageGenerator());

            switch (severity)
            {
                case LogEntrySeverity.Error:
                case LogEntrySeverity.Wtf:
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