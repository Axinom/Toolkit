namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;

	public static class ExtensionsForTask
	{
		/// <summary>
		/// Just a hint to the compiler that I know I am not doing anything with the task
		/// and it should shut up about it and not spam me with a warning.
		/// </summary>
		public static void Forget(this Task task)
		{
		}

		/// <summary>
		/// Signals that the continuation does not have to run using the current synchronization context.
		/// A more human-readable name for the commonly used ConfigureAwait(false) pattern.
		/// </summary>
		public static ConfiguredTaskAwaitable IgnoreContext(this Task task)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			return task.ConfigureAwait(false);
		}

		/// <summary>
		/// Signals that the continuation does not have to run using the current synchronization context.
		/// A more human-readable name for the commonly used ConfigureAwait(false) pattern.
		/// </summary>
		public static ConfiguredTaskAwaitable<T> IgnoreContext<T>(this Task<T> task)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			return task.ConfigureAwait(false);
		}

		/// <summary>
		/// Synchronously waits for the task to complete and unwraps any exceptions.
		/// Without using this, you get annoying AggregateExceptions that mess up all your error messages and stack traces.
		/// </summary>
		public static void WaitAndUnwrapExceptions(this Task task)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Synchronously waits for the task to complete and unwraps any exceptions.
		/// Without using this, you get annoying AggregateExceptions that mess up all your error messages and stack traces.
		/// </summary>
		public static T WaitAndUnwrapExceptions<T>(this Task<T> task)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			return task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Abandons the task after a cancellation is signaled and throws TaskCanceledException.
		/// The task continues to run in reality but will appear cancelled to any callers.
		/// </summary>
		/// <exception cref="TaskCanceledException">Thrown when the task is abandoned.</exception>
		public static Task<T> WithAbandonment<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			if (task.IsCompleted)
				return task;

			// From https://stackoverflow.com/questions/25219287/c-sharp-net-httpclient-cancel-readasstringasync

			return task.ContinueWith(completedTask => completedTask.WaitAndUnwrapExceptions(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		/// <summary>
		/// Abandons the task after a cancellation is signaled and throws TaskCanceledException.
		/// The task continues to run in reality but will appear cancelled to any callers.
		/// </summary>
		/// <exception cref="TaskCanceledException">Thrown when the task is abandoned.</exception>
		public static Task WithAbandonment(this Task task, CancellationToken cancellationToken)
		{
			Helpers.Argument.ValidateIsNotNull(task, nameof(task));

			if (task.IsCompleted)
				return task;

			// From https://stackoverflow.com/questions/25219287/c-sharp-net-httpclient-cancel-readasstringasync

			return task.ContinueWith(completedTask => completedTask.WaitAndUnwrapExceptions(), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}
	}
}