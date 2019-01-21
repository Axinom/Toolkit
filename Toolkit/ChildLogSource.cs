using System;

namespace Axinom.Toolkit
{
    /// <summary>
    /// A log source that is the child of another log source and offloads all the log messages to its parent.
    /// 
    /// This can be used to easily mark component ownership relations in logs, since it automatically appends
    /// the source name to the source name of the parent, without having to be manually configured (or even known).
    /// </summary>
    internal sealed class ChildLogSource : LogSource
    {
        private readonly LogSource _parent;

        public ChildLogSource(string name, LogSource parent) : base(name)
        {
            Helpers.Argument.ValidateIsNotNull(parent, nameof(parent));

            _parent = parent;
        }

        internal override void Write(LogEntrySeverity severity, string source, string message) => _parent.Write(severity, source, message);
        internal override void Write(LogEntrySeverity severity, string source, FormattableString message) => _parent.Write(severity, source, message);
    }
}