namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;

	/// <summary>
	/// Base class to coordinate global tasks like log system initialization.
	/// We don't use the "collection fixtures" because they force single-threaded test execution, which is not desired.
	/// </summary>
	public abstract class TestClass : IDisposable
	{
		private static readonly StreamWriter _logWriter;
		private static readonly object _coordinationLock = new object();

		static TestClass()
		{
			lock (_coordinationLock)
			{
				// If we have already created the log writer, we are set up and nothing more needs to be done.
				if (_logWriter != null)
					return;

				// This creates it in the same directory as the tests assembly, atleast using ReSharper and VSTS runners.
				// The stream is closed when the runner shuts down - we just flush more contents out to disk periodically.
				var logStream = File.Create("Tests.log");
				_logWriter = new StreamWriter(logStream);
				Log.Default.RegisterListener(new StreamWriterLogListener(_logWriter));
			}
		}

		public void Dispose()
		{
			// Not guaranteed to be thread-safe but uhh cross your fingers?! Seems to work okay, though.
			_logWriter.Flush();
		}
	}
}