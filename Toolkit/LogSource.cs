using System;

namespace Axinom.Toolkit
{
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

            var childName = name;

            if (_name.Length != 0)
                childName = $"{_name}/{childName}";

            return new ChildLogSource(childName, this);
        }

        public void Debug(string message)
        {
            Write(LogEntrySeverity.Debug, _name, message);
        }

        public void Info(string message)
        {
            Write(LogEntrySeverity.Info, _name, message);
        }

        public void Warning(string message)
        {
            Write(LogEntrySeverity.Warning, _name, message);
        }

        public void Error(string message)
        {
            Write(LogEntrySeverity.Error, _name, message);
        }

        public void Wtf(string message)
        {
            Write(LogEntrySeverity.Wtf, _name, message);
        }

        public void Debug(FormattableString message)
        {
            Write(LogEntrySeverity.Debug, _name, message);
        }

        public void Info(FormattableString message)
        {
            Write(LogEntrySeverity.Info, _name, message);
        }

        public void Warning(FormattableString message)
        {
            Write(LogEntrySeverity.Warning, _name, message);
        }

        public void Error(FormattableString message)
        {
            Write(LogEntrySeverity.Error, _name, message);
        }

        public void Wtf(FormattableString message)
        {
            Write(LogEntrySeverity.Wtf, _name, message);
        }

        protected LogSource(string name)
        {
            Helpers.Argument.ValidateIsNotNull(name, nameof(name));

            _name = name;
        }

        protected LogSource(LogSource wrapped)
        {
            Helpers.Argument.ValidateIsNotNull(wrapped, nameof(wrapped));

            _name = wrapped._name;
        }

        private readonly string _name;

        internal abstract void Write(LogEntrySeverity severity, string source, string message);
        internal abstract void Write(LogEntrySeverity severity, string source, FormattableString message);
    }
}