namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Logs the duration of an operation into the logging system debug output. Just for diagnostics.
	/// </summary>
	public sealed class DurationLogger : IDisposable
	{
		public DurationLogger(string operationName, LogSource logSource, bool isEnabled)
		{
			Helpers.Argument.ValidateIsNotNull(logSource, nameof(logSource));
			Helpers.Argument.ValidateIsNotNullOrWhitespace(operationName, nameof(operationName));

			_operationName = operationName;
			_logSource = logSource;

			// We enable the duration logging feature to be easily switched off via flag.
			if (!isEnabled)
				return;

			_stopwatch = new Stopwatch();
			_stopwatch.Start();

			_logSource.Debug($"{_operationName} - started.");
		}

		public DurationLogger(string operationName, LogSource logSource) : this(operationName, logSource, true)
		{
		}

		public void Dispose()
		{
			if (_stopwatch == null)
				return;

			_stopwatch.Stop();

			_logSource.Debug($"{_operationName} - completed in {_stopwatch.ElapsedMilliseconds}ms.");

			_stopwatch = null;
		}

		private readonly string _operationName;
		private readonly LogSource _logSource;

		private Stopwatch _stopwatch;
	}
}