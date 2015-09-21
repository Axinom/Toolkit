namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Windows.Foundation;

	public static class ExtensionsForIAsyncAction
	{
		/// <summary>
		/// Just a hint to the compiler that I know I am not doing anything with the task
		/// and it should shut up about it and not spam me with a warning.
		/// </summary>
		public static void Forget(this IAsyncAction action)
		{
		}

		/// <summary>
		/// Signals that the continuation does not have to run using the current synchronization context.
		/// A more human-readable name for the commonly used ConfigureAwait(false) pattern.
		/// </summary>
		public static ConfiguredTaskAwaitable IgnoreContext(this IAsyncAction action)
		{
			Helpers.Argument.ValidateIsNotNull(action, nameof(action));

			return action.AsTask().ConfigureAwait(false);
		}

		/// <summary>
		/// Signals that the continuation does not have to run using the current synchronization context.
		/// A more human-readable name for the commonly used ConfigureAwait(false) pattern.
		/// </summary>
		public static ConfiguredTaskAwaitable IgnoreContext<TProgress>(this IAsyncActionWithProgress<TProgress> action)
		{
			Helpers.Argument.ValidateIsNotNull(action, nameof(action));

			return action.AsTask().ConfigureAwait(false);
		}

		/// <summary>
		/// Synchronously waits for the task to complete and unwraps any exceptions.
		/// Without using this, you get annoying AggregateExceptions that mess up all your error messages and stack traces.
		/// </summary>
		public static void WaitAndUnwrapExceptions(this IAsyncAction action)
		{
			Helpers.Argument.ValidateIsNotNull(action, nameof(action));

			action.GetAwaiter().GetResult();
		}
	}
}