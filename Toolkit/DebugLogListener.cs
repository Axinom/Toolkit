// We always want debug output enabled for this file, otherwise this class is useless.
#define DEBUG

namespace Axinom.Toolkit
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Writes log entries to debugger output.
    /// </summary>
    public sealed class DebugLogListener : ILogListener
    {
        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            var message = string.Format("{0} {1:u} [{2}]" + Environment.NewLine + "{3}",
                severity, timestamp, source, messageGenerator());

            Debug.WriteLine(message);
        }

        public void Dispose()
        {
        }
    }
}