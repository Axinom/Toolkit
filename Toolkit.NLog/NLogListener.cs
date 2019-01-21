namespace Axinom.Toolkit
{
    using global::NLog;
    using System;

    /// <summary>
    /// A log entry listener that forwards all log entries to an NLog logger with the same name as the Axinom Toolkit log source.
    /// </summary>
    public sealed class NLogListener : ILogListener
    {
        /// <summary>
        /// Initializes a listener that uses the provided log factory to create loggers.
        /// The listener does not take ownership of (or dispose of) the factory when the listener is disposed of.
        /// </summary>
        public NLogListener(LogFactory logFactory)
        {
            Helpers.Argument.ValidateIsNotNull(logFactory, "logFactory");

            _logFactory = logFactory;
        }

        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            var logger = _logFactory.GetLogger(source ?? "");
            logger.Log(TranslateLogLevel(severity), new LogMessageGenerator(messageGenerator));
        }

        private LogLevel TranslateLogLevel(LogEntrySeverity level)
        {
            switch (level)
            {
                case LogEntrySeverity.Debug:
                    return LogLevel.Debug;
                case LogEntrySeverity.Info:
                    return LogLevel.Info;
                case LogEntrySeverity.Warning:
                    return LogLevel.Warn;
                case LogEntrySeverity.Error:
                    return LogLevel.Error;
                case LogEntrySeverity.Wtf:
                    return LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
        }

        private readonly LogFactory _logFactory;

        public void Dispose()
        {
        }
    }
}