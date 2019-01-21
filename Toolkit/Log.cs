namespace Axinom.Toolkit
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// General purpose lightweight logging class. Not super optimized but probably good enough.
    /// Register one or more log listeners to actually make the log entries go somewhere.
    /// </summary>
    /// <remarks>
    /// Thread-safe - this object may be used from any thread.
    /// 
    /// You should call Dispose() before exiting the application to dispose of the log listeners.
    /// </remarks>
    public sealed class Log : LogSource, IDisposable
    {
        /// <summary>
        /// Gets the default log instance, which accepts listener registrations and is the top-level owner of child log sources.
        /// </summary>
        public static Log Default { get; } = new Log();

        private readonly List<ILogListener> _listeners = new List<ILogListener>();
        private readonly object _lock = new object();

        /// <summary>
        /// Registers a log listener to receive log entries. It will automatically be disposed of when the application exits.
        /// </summary>
        public void RegisterListener(ILogListener listener)
        {
            Helpers.Argument.ValidateIsNotNull(listener, "listener");

            lock (_lock)
                _listeners.Add(listener);
        }

        public void UnregisterListener(ILogListener listener)
        {
            Helpers.Argument.ValidateIsNotNull(listener, "listener");

            lock (_lock)
            {
                _listeners.Remove(listener);

                listener.Dispose();
            }
        }

        #region Init & deinit
        /// <summary>
        /// Do not use. Internal for testing purposes only.
        /// </summary>
        internal Log() : base("")
        {
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var listener in _listeners.ToArray())
                {
                    _listeners.Remove(listener);

                    listener.Dispose();
                }
            }
        }
        #endregion

        private void Write(LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
            var timestamp = DateTimeOffset.UtcNow;
            var generatorWrapper = new Lazy<string>(delegate
            {
                try
                {
                    return messageGenerator();
                }
                catch (Exception ex)
                {
                    return $"Failed to generate log message: {ex}";
                }
            });

            // The lock is here just to ensure that no listener gets removed/disposed while it is being written to.
            lock (_lock)
            {
                foreach (var listener in _listeners)
                {
                    try
                    {
                        listener.OnWrite(timestamp, severity, source, () => generatorWrapper.Value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                };
            }
        }

        internal override void Write(LogEntrySeverity severity, string source, string message)
        {
            Write(severity, source, () => message);
        }

        internal override void Write(LogEntrySeverity severity, string source, FormattableString message)
        {
            Write(severity, source, message.ToString);
        }
    }
}