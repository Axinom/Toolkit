namespace Axinom.Toolkit
{
    using System;

    /// <summary>
    /// A log listener that wraps another log listener and filters the entries it passes to it.
    /// </summary>
    public sealed class FilteringLogListener : ILogListener
    {
        /// <summary>
        /// Gets or sets the minimum log level severity required to let an entry through the filter.
        /// </summary>
        public LogEntrySeverity MinimumSeverity { get; set; }

        public FilteringLogListener(ILogListener inner)
        {
            Helpers.Argument.ValidateIsNotNull(inner, nameof(inner));

            _inner = inner;
        }

        private readonly ILogListener _inner;

        public void Dispose()
        {
            _inner.Dispose();
        }

        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            if (severity < MinimumSeverity)
                return;

            _inner.OnWrite(timestamp, severity, source, messageGenerator);
        }
    }
}