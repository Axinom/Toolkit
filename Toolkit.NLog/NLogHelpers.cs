﻿namespace Axinom.Toolkit
{
    using global::NLog;
    using global::NLog.Config;
    using global::NLog.Targets;

    /// <summary>
    /// Helper methods related to NLog.
    /// </summary>
    public static class NLogHelpers
    {
        /// <summary>
        /// A NLog layout that just prints out the message and nothing else.
        /// </summary>
        private const string OnlyMessageLayout = "${message}";

        /// <summary>
        /// Sets up an NLog configuration that stores all messages in the MemoryTarget instance returned by this method.
        /// </summary>
        public static MemoryTarget SetupCapturingLoggingConfiguration(this HelpersContainerClasses.NLog container, LogFactory logFactory, LogLevel minLevel = null,
            string layout = OnlyMessageLayout)
        {
            const string targetName = "TestTarget";

            var configuration = new LoggingConfiguration();

            var memoryTarget = new MemoryTarget();
            configuration.AddTarget(targetName, memoryTarget);

            memoryTarget.Layout = layout;

            var loggingRule = new LoggingRule("*", minLevel ?? LogLevel.Trace, memoryTarget);
            configuration.LoggingRules.Add(loggingRule);

            logFactory.Configuration = configuration;

            return memoryTarget;
        }
    }
}