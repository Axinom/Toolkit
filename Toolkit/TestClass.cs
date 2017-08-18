namespace Axinom.Toolkit
{
    using System;
    using System.IO;

    /// <summary>
    /// Base class to coordinate global tasks like log system initialization in automated test projects.
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

                Stream logStream;

                try
                {
                    logStream = File.Open("Tests.log", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                }
                catch (UnauthorizedAccessException)
                {
                    logStream = File.Open(Path.Combine(Path.GetTempPath(), "Tests.log"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                }

                _logWriter = new StreamWriter(logStream)
                {
                    AutoFlush = true
                };

                Log.Default.RegisterListener(new StreamWriterLogListener(_logWriter));
            }
        }

        public static void EnsureInitialized()
        {
        }

        public virtual void Dispose()
        {
            // Not guaranteed to be thread-safe but uhh cross your fingers?! Seems to work okay, though.
            _logWriter.Flush();
        }
    }
}