using System;

namespace Axinom.Toolkit
{
    /// <summary>
    /// Implements a thread-safe log listener that accepts log entries that come from various log sources.
    /// The log listener will be disposed of automatically when it is unregistered or when the appplication exits.
    /// </summary>
    public interface ILogListener : IDisposable
    {
        /// <summary>
        /// Notifies the listener that an entry has been written to the log.
        /// </summary>
        void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator);
    }
}