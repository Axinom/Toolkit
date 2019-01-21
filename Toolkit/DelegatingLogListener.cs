using System;

namespace Axinom.Toolkit
{
	/// <summary>
	/// A simple log listener that shovels log lines off to a basic Write() method or similar.
	/// </summary>
	public sealed class DelegatingLogListener : ILogListener
	{
		private readonly Action<FormattableString> _writeLineMethod;

		public DelegatingLogListener(Action<string> writeLineMethod)
		{
			Helpers.Argument.ValidateIsNotNull(writeLineMethod, nameof(writeLineMethod));

			_writeLineMethod = delegate (FormattableString fs)
			{
				writeLineMethod(fs.ToString());
			};
		}

		public DelegatingLogListener(Action<FormattableString> writeLineMethod)
		{
			Helpers.Argument.ValidateIsNotNull(writeLineMethod, nameof(writeLineMethod));

			_writeLineMethod = writeLineMethod;
		}

        public void OnWrite(DateTimeOffset timestamp, LogEntrySeverity severity, string source, Func<string> messageGenerator)
        {
			_writeLineMethod($"{severity} [{source}] {messageGenerator()} (@{timestamp:u})");
		}

		public void Dispose()
		{
		}
	}
}
