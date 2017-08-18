namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// A lock construct that is based on a SemaphoreSlim instance. Supports asyncronous and multi-threaded use.
	/// Each "lock" is one decrement/increment pair for a semaphore. The idea is to allow semaphores to be used as async-safe locks.
	/// </summary>
	public sealed class SemaphoreLock : IDisposable
	{
		public static SemaphoreLock Take(SemaphoreSlim semaphore)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			semaphore.Wait();

			return new SemaphoreLock(semaphore);
		}

		public static async Task<SemaphoreLock> TakeAsync(SemaphoreSlim semaphore)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			await semaphore.WaitAsync().IgnoreContext();

			return new SemaphoreLock(semaphore);
		}

		public static SemaphoreLock Take(SemaphoreSlim semaphore, CancellationToken cancel)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			semaphore.Wait(cancel);

			return new SemaphoreLock(semaphore);
		}

		public static async Task<SemaphoreLock> TakeAsync(SemaphoreSlim semaphore, CancellationToken cancel)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			await semaphore.WaitAsync(cancel).IgnoreContext();

			return new SemaphoreLock(semaphore);
		}

		public static SemaphoreLock TryTake(SemaphoreSlim semaphore, TimeSpan timeout)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			if (semaphore.Wait(timeout))
				return new SemaphoreLock(semaphore);
			else
				return null;
		}

		public static async Task<SemaphoreLock> TryTakeAsync(SemaphoreSlim semaphore, TimeSpan timeout)
		{
			Helpers.Argument.ValidateIsNotNull(semaphore, nameof(semaphore));

			if (await semaphore.WaitAsync(timeout).IgnoreContext())
				return new SemaphoreLock(semaphore);
			else
				return null;
		}

		private readonly SemaphoreSlim _semaphore;

		private SemaphoreLock(SemaphoreSlim semaphore)
		{
			_semaphore = semaphore;
		}

		public void Dispose()
		{
			_semaphore.Release();
		}
	}
}