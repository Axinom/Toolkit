namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// A log source that is the child of another log source and offloads all the log messages to its parent.
	/// This can be used to easily mark component ownership relations in logs, since it automatically appends
	/// the source name to the source name of the parent, without having to be manually configured (or even known).
	/// </summary>
	internal sealed class ChildLogSource : LogSource
	{
		private readonly string _name;
		private readonly LogSource _parent;

		public ChildLogSource(string name, LogSource parent)
		{
			_name = name;
			_parent = parent;
		}

		internal override void Write(LogEntrySeverity severity, string originalSource, string message, params object[] args) => _parent.Write(severity, FormatSourceString(originalSource), message, args);

		internal override void Write(LogEntrySeverity severity, string originalSource, FormattableString message) => _parent.Write(severity, FormatSourceString(originalSource), message);

		// This event might be coming from a child log or this one. If this one, it does not yet have the source name in there.
		// Prefix original source with current source name or just take current source name if there is no original.
		private string FormatSourceString(string originalSource) => originalSource == null ? _name : _name + "/" + originalSource;
	}
}