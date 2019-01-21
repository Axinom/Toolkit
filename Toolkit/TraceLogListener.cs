namespace Axinom.Toolkit
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Writes log entries to trace output.
    /// </summary>
    public sealed class TraceLogListener : ILogListener
    {
        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
                severity, timestamp, source, messageGenerator());

            switch (severity)
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