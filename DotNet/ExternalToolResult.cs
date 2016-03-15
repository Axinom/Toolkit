namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// The result of executing an instance of an external tool.
	/// Available once the external tool has finished its work.
	/// </summary>
	public sealed class ExternalToolResult
	{
		public ExternalTool.Instance Instance { get; private set; }

		public bool Succeeded { get; private set; }

		public string StandardOutput { get; private set; }
		public string StandardError { get; private set; }

		public int ExitCode { get; private set; }

		public TimeSpan Duration { get; private set; }

		/// <summary>
		/// Forwards the external tool's standard output to the current app's standard output.
		/// </summary>
		public void ForwardOutputs()
		{
			if (!string.IsNullOrWhiteSpace(StandardOutput))
				_log.Debug(StandardOutput);

			if (!string.IsNullOrWhiteSpace(StandardError))
			{
				// NB! We log standard error to standard output if we did not detect any failure.
				// This is done because some tools accidentally log to stderr when they should log to stdout.
				// This behavior is controlled by the results heuristics settings in the ExternalTool template.

				if (Succeeded)
				{
					_log.Debug("Treating stderr as stdout since result heuristics did not consider the result a failure.");
					_log.Debug(StandardError);
				}
				else
				{
					_log.Error(StandardError);
				}
			}
		}

		/// <summary>
		/// Consumes the result. This forwards the output and throws an exception if the tool execution failed.
		/// </summary>
		public void Consume()
		{
			_log.Debug("Processing output of \"{0}\" {1}", Instance.ExecutablePath, Instance.Arguments);

			ForwardOutputs();
			VerifySuccess();

			_log.Debug("Finished in {0:F2}s.", Duration.TotalSeconds);
		}

		/// <summary>
		/// Verifies that the tool execution was successful. Throws an exception if any failure occurred.
		/// </summary>
		public void VerifySuccess()
		{
			if (!Succeeded)
				throw new EnvironmentException(string.Format("External tool failure detected! Command: \"{0}\" {1}; Exit code: {2}; Runtime: {3:F2}s.", Instance.ExecutablePath, Instance.Arguments, ExitCode, Duration.TotalSeconds));
		}

		#region Implementation details
		private readonly LogSource _log;

		internal ExternalToolResult(ExternalTool.Instance externalToolInstance, string standardOutput, string standardError, int exitCode, TimeSpan duration)
		{
			Instance = externalToolInstance;
			StandardOutput = standardOutput;
			StandardError = standardError;
			ExitCode = exitCode;
			Duration = duration;

			Succeeded = DetermineSuccess();

			_log = Log.Default.CreateChildSource(Path.GetFileName(externalToolInstance.ExecutablePath));
		}

		/// <summary>
		/// Detects whether any failures occurred during external tool usage.
		/// </summary>
		private bool DetermineSuccess()
		{
			if (ExitCode != 0)
				return false;

			if (!Instance.StandardErrorIsNotError)
			{
				// Any output in standard error stream is a failure.
				if (!string.IsNullOrWhiteSpace(StandardError))
					return false;
			}

			return true;
		}
		#endregion
	}
}