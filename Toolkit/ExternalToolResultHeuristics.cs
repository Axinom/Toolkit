namespace Axinom.Toolkit
{
    /// <summary>
    /// Enables the behavior of the result processing system to be tuned for a particular tool.
    /// </summary>
    public sealed class ExternalToolResultHeuristics
    {
        /// <summary>
        /// Some tools send output to the standard error stream even when there is no error occurring.
        /// Turn on this flag to enable treating the standard error stream as just a secondary output stream.
        /// Defaults to false - any output in standard error stream is treated as a failure.
        /// </summary>
        public bool StandardErrorIsNotError { get; set; }

        public static readonly ExternalToolResultHeuristics Default = new ExternalToolResultHeuristics
        {
            StandardErrorIsNotError = false
        };

        public static readonly ExternalToolResultHeuristics Linux = new ExternalToolResultHeuristics
        {
            StandardErrorIsNotError = true
        };
    }
}