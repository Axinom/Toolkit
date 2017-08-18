﻿namespace Axinom.Toolkit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static partial class CoreHelpers
    {
        /// <summary>
        /// Asynchronously invokes a long-running action on a new background thread.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
        public static void BackgroundThreadInvoke(this HelpersContainerClasses.Async container, Action action)
        {
            Helpers.Argument.ValidateIsNotNull(action, nameof(action));

            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}