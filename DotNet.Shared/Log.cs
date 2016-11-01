namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// General purpose lightweight logging class. Register one or more log listeners to actually make the log entries go somewhere.
	/// The class is not very optimized and you will find more luck with something like NLog in all but the most trivial cases.
	/// Thread-safe - this object may be used from any thread.
	/// </summary>
	/// <remarks>
	/// You should call Dispose() before exiting the application to dispose of the log listeners and ensure they clean up properly.
	/// </remarks>
	public sealed class Log : LogSource, IDisposable
	{
		#region Singleton
		private static readonly Log _default = new Log();

		/// <summary>
		/// Gets the default log instance, which accepts listener registrations and is the top-level owner of child log sources.
		/// </summary>
		public static Log Default
		{
			get { return _default; }
		}
		#endregion

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
		internal Log()
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

		internal override void Write(LogEntrySeverity severity, string originalSource, string message, params object[] args)
		{
			var finalMessage = message;

			try
			{
				if (args.Length != 0)
				{
					// There could easily be errors in here. We fall back to unformatted text on any failure.
					finalMessage = string.Format(message, args);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error while formatting log message text: " + ex.Message);
			}

			var entry = new LogEntry(severity, finalMessage, originalSource);

			lock (_lock)
			{
				foreach (var listener in _listeners)
				{
					try
					{
						listener.OnWrite(entry);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine("ILogListener.OnWrite() threw exception: " + ex);
					}
				}
			}
		}

		internal override void Write(LogEntrySeverity severity, string originalSource, FormattableString message)
		{
			Write(severity, originalSource, message.ToString());
		}
	}
}