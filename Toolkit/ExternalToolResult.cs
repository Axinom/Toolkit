namespace Axinom.Toolkit
{
    using System;
    using System.IO;

    /// <summary>
    /// The result of executing an instance of an external tool.
    /// Available once the external tool has finished its work.
    /// </summary>
    public sealed class ExternalToolResult
    {
        public ExternalTool.Instance Instance { get; }

        public bool Succeeded { get; }

        public string StandardOutput { get; }
        public string StandardError { get; }

        public int ExitCode { get; }

        public TimeSpan Duration { get; }

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
            {
                // We report first 1 KB of stderr or stdout, to provide extra information if available.
                var detailsSource = (!string.IsNullOrWhiteSpace(StandardError) ? StandardError : StandardOutput) ?? "";
                var details = detailsSource.Substring(0, Math.Min(detailsSource.Length, 1024));

                throw new EnvironmentException($"External tool failure detected! Command: \"{Instance.ExecutablePath}\" {Instance.CensoredArguments}; Exit code: {ExitCode}; Runtime: {Duration.TotalSeconds:F2}s. Head of output: {details}");
            }
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