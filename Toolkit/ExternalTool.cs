namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// An external tool (exe, bat or other) that can be executed by automation when needed.
	/// Provides standard functionality such as output capture and reacting to meaningful results (e.g. throw on error).
	/// </summary>
	/// <remarks>
	/// To use, create an instance, fill the properties and call Start/ExecuteAsync.
	/// </remarks>
	public sealed class ExternalTool
	{
		/// <summary>
		/// There are various operations that should complete near-instantly but for
		/// reasons of operating systems magic may hang. This timeout controls when we give up.
		/// </summary>
		private static readonly TimeSpan LastResortTimeout = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Absolute or relative path to the executable. Relative paths are resolved mostly
		/// according to OS principles (PATH environment variable and potentially some others).
		/// </summary>
		public string ExecutablePath { get; set; }

		/// <summary>
		/// Arguments string to provide to the executable.
		/// </summary>
		public string Arguments { get; set; }

		/// <summary>
		/// Any environment variables to add to the normal environment variable set.
		/// </summary>
		public Dictionary<string, string> EnvironmentVariables { get; set; }

		/// <summary>
		/// Defaults to the working directory of the current process.
		/// </summary>
		public string WorkingDirectory { get; set; }

		/// <summary>
		/// Copies the standard output and standard error streams to the specified file if set.
		/// </summary>
		public string OutputFilePath { get; set; }

		/// <summary>
		/// Enables the result processing logic to be fine-tuned.
		/// </summary>
		public ExternalToolResultHeuristics ResultHeuristics { get; set; }

		/// <summary>
		/// Allows a custom action to consume data from the standard output stream.
		/// Note that this will make standard output stream contents invisible to ExternalTool.
		/// The action is executed on a dedicated thread.
		/// </summary>
		public Action<Stream> StandardOutputConsumer { get; set; }

		/// <summary>
		/// Allows a custom action to consume data from the standard error stream.
		/// Note that this will make standard error stream contents invisible to ExternalTool.
		/// The action is executed on a dedicated thread.
		/// </summary>
		public Action<Stream> StandardErrorConsumer { get; set; }

		/// <summary>
		/// Allows a custom action to provide data on the standard input stream.
		/// The action is executed on a dedicated thread.
		/// </summary>
		public Action<Stream> StandardInputProvider { get; set; }

		/// <summary>
		/// If set, any strings in this collection are censored in log output (though not stdout/stderr).
		/// Useful if you pass credentials on the command line.
		/// </summary>
		public IReadOnlyCollection<string> CensoredStrings { get; set; }

		/// <summary>
		/// Starts a new instance of the external tool. Use this if you want more detailed control over the process
		/// e.g. the ability to terminate it or to inspect the running process. Otherwise, just use the synchronous Execute().
		/// </summary>
		public Instance Start()
		{
			var instance = new Instance(this);
			instance.Start();
			return instance;
		}

		/// <summary>
		/// Synchronously executes an instance of the external tool and consumes the result.
		/// </summary>
		public ExternalToolResult Execute(TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new TimeoutException("The external tool could not be executed because the operation had already timed out.");

			var instance = Start();
			var result = instance.GetResult(timeout);

			result.Consume();

			return result;
		}

        /// <summary>
        /// Asynchronously executes an instance of the external tool and consumes the result.
        /// </summary>
        public async Task<ExternalToolResult> ExecuteAsync(CancellationToken cancel = default)
        {
            cancel.ThrowIfCancellationRequested();

            var instance = Start();
            var result = await instance.GetResultAsync(cancel);

            result.Consume();

            return result;
        }

        /// <summary>
        /// Helper method to quickly execute a command with arguments and consume the result.
        /// </summary>
        public static ExternalToolResult Execute(string executablePath, string arguments, TimeSpan timeout)
		{
			Helpers.Argument.ValidateIsNotNullOrWhitespace(executablePath, "executablePath");

			return new ExternalTool
			{
				ExecutablePath = executablePath,
				Arguments = arguments
			}.Execute(timeout);
		}

        /// <summary>
        /// Helper method to quickly execute a command with arguments and consume the result.
        /// </summary>
        public static Task<ExternalToolResult> ExecuteAsync(string executablePath, string arguments, CancellationToken cancel = default)
        {
            Helpers.Argument.ValidateIsNotNullOrWhitespace(executablePath, "executablePath");

            return new ExternalTool
            {
                ExecutablePath = executablePath,
                Arguments = arguments
            }.ExecuteAsync(cancel);
        }

        public ExternalTool()
		{
			EnvironmentVariables = new Dictionary<string, string>();
		}

		/// <summary>
		/// A started instance of an external tool. May have finished running already.
		/// </summary>
		public sealed class Instance
		{
			public Process Process { get; private set; }

			public string ExecutablePath { get; private set; }
			public string Arguments { get; private set; }
			public string CensoredArguments { get; private set; }
			public IReadOnlyDictionary<string, string> EnvironmentVariables { get; private set; }
			public string WorkingDirectory { get; private set; }
			public string OutputFilePath { get; private set; }

			// Copied values from result heuristics of the template.
			public bool StandardErrorIsNotError { get; private set; }

			/// <summary>
			/// Waits for the tool to exit and retrieves the result.
            /// If a timeout occurs, the running external tool process is killed.
			/// </summary>
			/// <exception cref="TimeoutException">Thrown if a timeout occurs.</exception>
			public ExternalToolResult GetResult(TimeSpan timeout)
			{
				if (!_result.Task.Wait(timeout))
				{
                    _log.Debug("Terminating external tool due to timeout.");

					Process.Kill();

					// Wait for result to be available so that all the output gets written to file.
					// This may not work if something is very wrong, but we do what we can to help.
					_result.Task.Wait(LastResortTimeout);

					throw new TimeoutException(string.Format("Timeout waiting for external tool to finish: \"{0}\" {1}", ExecutablePath, Arguments));
				}

				return _result.Task.WaitAndUnwrapExceptions();
			}

            /// <summary>
			/// Waits for the tool to exit and retrieves the result.
            /// If the cancellation token is cancelled, the running external tool process is killed.
			/// </summary>
            public async Task<ExternalToolResult> GetResultAsync(CancellationToken cancel = default)
            {
                try
                {
                    return await _result.Task.WithAbandonment(cancel);
                }
                catch (TaskCanceledException)
                {
                    _log.Debug("Terminating external tool due to cancellation.");

                    // If a cancellation is signaled, we need to kill the process and set error to really time it out.
                    Process.Kill();

                    // Wait for result to be available so that all the output gets written to file.
                    // This may not work if something is very wrong, but we do what we can to help.
                    _result.Task.Wait(LastResortTimeout);

                    throw new TaskCanceledException(string.Format("External tool execution cancelled: \"{0}\" {1}", ExecutablePath, Arguments));
                }
            }

			private Action<Stream> _standardOutputConsumer;
			private Action<Stream> _standardInputProvider;
			private Action<Stream> _standardErrorConsumer;

            private readonly TaskCompletionSource<ExternalToolResult> _result = new TaskCompletionSource<ExternalToolResult>();

			private readonly LogSource _log;

			/// <summary>
			/// Creates a new instance of an external tool, using the specified template. Does not start it yet.
			/// </summary>
			internal Instance(ExternalTool template)
			{
				Helpers.Argument.ValidateIsNotNull(template, nameof(template));

				if (string.IsNullOrWhiteSpace(template.ExecutablePath))
					throw new ArgumentException("Executable path must be specified.", nameof(template));

				if (template.WorkingDirectory != null && !Directory.Exists(template.WorkingDirectory))
					throw new ArgumentException("The working directory does not exist.", nameof(template));

				_log = Log.Default.CreateChildSource(Path.GetFileName(template.ExecutablePath));

				var executablePath = template.ExecutablePath;

				// First, resolve the path.
				if (!Path.IsPathRooted(executablePath))
				{
					var resolvedPath = Helpers.Filesystem.ResolvePath(executablePath);
					_log.Debug("Executable resolved to {0}", resolvedPath);

					executablePath = resolvedPath;
				}

				// Then prepare the variables.
				ExecutablePath = executablePath;
				Arguments = template.Arguments ?? "";
				EnvironmentVariables = new Dictionary<string, string>(template.EnvironmentVariables ?? new Dictionary<string, string>());
				WorkingDirectory = template.WorkingDirectory ?? Environment.CurrentDirectory;
				OutputFilePath = template.OutputFilePath;

				var heuristics = template.ResultHeuristics ?? ExternalToolResultHeuristics.Default;
				StandardErrorIsNotError = heuristics.StandardErrorIsNotError;

				_standardInputProvider = template.StandardInputProvider;
				_standardOutputConsumer = template.StandardOutputConsumer;
				_standardErrorConsumer = template.StandardErrorConsumer;

				// We may need to censor the log line!
				CensoredArguments = Arguments;

				if (template.CensoredStrings?.Count > 0)
				{
					foreach (var censoredString in template.CensoredStrings)
					{
						if (string.IsNullOrWhiteSpace(censoredString))
							continue;

						CensoredArguments = CensoredArguments.Replace(censoredString, "*********");
					}
				}
			}

			/// <summary>
			/// We want to suppress any Windows error reporting dialogs that occur due to the external tool crashing.
			/// During the lifetime of this object, this is done for the current process and any started process
			/// will inherit this configuration from the current process, so ensure this object is alive during child start.
			/// 
			/// Note that this class also acts as a mutex.
			/// </summary>
			/// <remarks>
			/// This class does nothing on non-Windows operating systems.
			/// </remarks>
			private sealed class CrashDialogSuppressionBlock : IDisposable
			{
				public CrashDialogSuppressionBlock()
				{
					if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
						return;

					Monitor.Enter(_errorModeLock);

					// Keep any default flags the OS gives us and ensure that our own flags are added.
					_previousErrorMode = GetErrorMode();
					SetErrorMode(_previousErrorMode.Value | ErrorModes.SEM_FAILCRITICALERRORS | ErrorModes.SEM_NOGPFAULTERRORBOX);
				}

				private ErrorModes? _previousErrorMode;

				public void Dispose()
				{
					if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
						return;

					// Can only dispose once.
					if (_previousErrorMode == null)
						return;

					SetErrorMode(_previousErrorMode.Value);
					_previousErrorMode = null;

					Monitor.Exit(_errorModeLock);
				}

				[DllImport("kernel32.dll")]
				private static extern ErrorModes SetErrorMode(ErrorModes uMode);

				[DllImport("kernel32.dll")]
				private static extern ErrorModes GetErrorMode();

				[Flags]
				private enum ErrorModes : uint
				{
					// ReSharper disable InconsistentNaming
					SYSTEM_DEFAULT = 0x0,
					SEM_FAILCRITICALERRORS = 0x0001,
					SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
					SEM_NOGPFAULTERRORBOX = 0x0002,
					SEM_NOOPENFILEERRORBOX = 0x8000
					// ReSharper restore InconsistentNaming
				}

				/// <summary>
				/// We only want one thread to be touching the error mode at the same time.
				/// </summary>
				private static readonly object _errorModeLock = new object();
			}

			internal void Start()
			{
				if (Process != null)
					throw new InvalidOperationException("The instance has already been started.");

				_log.Debug("Executing: \"{0}\" {1}", ExecutablePath, CensoredArguments);

				StreamWriter outputFileWriter = null;

				if (!string.IsNullOrWhiteSpace(OutputFilePath))
				{
					// Make sure the file can be created - parent directory exists.
					var parent = Path.GetDirectoryName(OutputFilePath);

					// No need to create it if it is a relative path with no parent.
					if (!string.IsNullOrWhiteSpace(parent))
						Directory.CreateDirectory(parent);

					// Create the file.
					outputFileWriter = File.CreateText(OutputFilePath);
				}

				try
				{
					var startInfo = new ProcessStartInfo
					{
						Arguments = Arguments,
						ErrorDialog = false,
						FileName = ExecutablePath,
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						WorkingDirectory = WorkingDirectory,
						CreateNoWindow = true
					};

					if (EnvironmentVariables != null)
					{
						foreach (var pair in EnvironmentVariables)
							startInfo.EnvironmentVariables[pair.Key] = pair.Value;
					}

					if (_standardInputProvider != null)
						startInfo.RedirectStandardInput = true;

					string standardError = null;
					string standardOutput = null;

                    var runtime = Stopwatch.StartNew();

					using (new CrashDialogSuppressionBlock())
						Process = Process.Start(startInfo);

                    // We default all external tools to below normal because they are, as a rule, less
                    // important than fast responsive UX, so the system should not be bogged down by them.
                    try
                    {
                        Process.PriorityClass = ProcessPriorityClass.BelowNormal;
                    }
                    catch (InvalidOperationException)
                    {
                        // If the process has already exited, this will throw IOE. That is fine.
                    }

					// These are only set if they are created by ExternalTool - we don't care about user threads.
					Thread standardErrorReader = null;
					Thread standardOutputReader = null;

					if (_standardErrorConsumer != null)
					{
						// Caller wants to have it. Okay, fine.
						Helpers.Async.BackgroundThreadInvoke(delegate { _standardErrorConsumer(Process.StandardError.BaseStream); });
					}
					else
					{
						// We'll store it ourselves.
						standardErrorReader = new Thread((ThreadStart)delegate
						{
							// This should be safe if the process we are starting is well-behaved (i.e. not ADB).
							standardError = Process.StandardError.ReadToEnd();
						});

						standardErrorReader.Start();
					}

					if (_standardOutputConsumer != null)
					{
						// Caller wants to have it. Okay, fine. We do not need to track this thread.
						Helpers.Async.BackgroundThreadInvoke(delegate { _standardOutputConsumer(Process.StandardOutput.BaseStream); });
					}
					else
					{
						// We'll store it ourselves.
						standardOutputReader = new Thread((ThreadStart)delegate
						{
							// This should be safe if the process we are starting is well-behaved (i.e. not ADB).
							standardOutput = Process.StandardOutput.ReadToEnd();
						});

						standardOutputReader.Start();
					}


					if (_standardInputProvider != null)
					{
						// We don't care about monitoring this later, since ExternalTool does not need to touch stdin.
						Helpers.Async.BackgroundThreadInvoke(delegate
                        {
                            // Closing stdin after providing input is critical or the app may just hang forever.
                            using (var stdin = Process.StandardInput.BaseStream)
                                _standardInputProvider(stdin);
                        });
					}

					var resultThread = new Thread((ThreadStart)delegate
					{
						Process.WaitForExit();
						runtime.Stop();

                        // NB! Streams may stay open and blocked after process exits.
                        // This happens e.g. if you go cmd.exe -> start.exe.
                        // Even if you kill cmd.exe, start.exe remains and keeps the pipes open.
                        standardErrorReader?.Join();
                        standardOutputReader?.Join();

						if (outputFileWriter != null)
						{
							if (standardOutput != null)
								outputFileWriter.WriteLine(standardOutput);

							if (standardError != null)
								outputFileWriter.WriteLine(standardError);

							outputFileWriter.Dispose();
						}

                        _result.TrySetResult(new ExternalToolResult(this, standardOutput, standardError, Process.ExitCode, runtime.Elapsed));
					});

					// All the rest happens in the result thread, which waits for the process to exit.
					resultThread.Start();
				}
				catch (Exception)
				{
					// Don't leave this lingering if starting the process fails.
					outputFileWriter?.Dispose();

					throw;
				}
			}
		}
	}
}