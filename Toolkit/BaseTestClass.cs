namespace Axinom.Toolkit
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Base class to coordinate global tasks like log system initialization in automated test projects.
    /// </summary>
    public abstract class BaseTestClass : IDisposable
    {
        private static readonly StreamWriter _logWriter;
        private static readonly object _coordinationLock = new object();

        static BaseTestClass()
        {
            lock (_coordinationLock)
            {
                // If we have already created the log writer, we are set up and nothing more needs to be done.
                if (_logWriter != null)
                    return;

                // In VSTS automated build processes, Console.InputEncoding is UTF-8 with byte order mark.
                // This causes this encoding to be propagated in Process.Start() which means we get BOMs
                // whenever we write to stdin anywhere. Terrible idea - we get rid of the BOM here!
                Console.InputEncoding = new UTF8Encoding(false);

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