namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class SemaphoreLockTests
	{
		[Fact]
		public void BasicLockingLogic_SeemsToWork()
		{
			// Start one long-running operation and one short-running one.
			// Expected behavior is that short one will wait for long one to complete.

			using (var semaphore = new SemaphoreSlim(1))
			{
				int? longCompletedAt = null;
				int? shortCompletedAt = null;

				var longTask = Task.Run(async () =>
				{
					Debug.WriteLine("Long entered.");
					using (await SemaphoreLock.TakeAsync(semaphore))
					{
						Debug.WriteLine("Long acquired lock.");
						await Task.Delay(5000);
						Debug.WriteLine("Long completed.");
						longCompletedAt = Environment.TickCount;
					}
				});

				var shortTask = Task.Run(async () =>
				{
					Debug.WriteLine("Short entered.");
					await Task.Delay(500);
					Debug.WriteLine("Short starting work.");
					using (await SemaphoreLock.TakeAsync(semaphore))
					{
						Debug.WriteLine("Short acquired lock.");
						await Task.Delay(50);
						Debug.WriteLine("Short completed.");
						shortCompletedAt = Environment.TickCount;
					}
				});

				Task.WaitAll(longTask, shortTask);

				Assert.True(shortCompletedAt.HasValue);
				Assert.True(longCompletedAt.HasValue);
				Assert.True(longCompletedAt.Value < shortCompletedAt.Value);
			}
		}

		[Fact]
		public void LockingLogic_WithTimeout_SeemsToWork()
		{
			// Start one long-running operation and two short-running ones, where one of the shorts times out on the lock.

			using (var semaphore = new SemaphoreSlim(1))
			{
				int? longCompletedAt = null;
				int? short1CompletedAt = null;
				int? short2CompletedAt = null;
				int? short1TimedOutAt = null;

				var longTask = Task.Run(async () =>
				{
					Debug.WriteLine("Long entered.");
					using (await SemaphoreLock.TakeAsync(semaphore))
					{
						Debug.WriteLine("Long acquired lock.");
						await Task.Delay(5000);
						Debug.WriteLine("Long completed.");
						longCompletedAt = Environment.TickCount;
					}
				});

				var shortTask1 = Task.Run(async () =>
				{
					Debug.WriteLine("Short1 entered.");
					await Task.Delay(500);
					Debug.WriteLine("Short1 starting work.");
					var lockInstance = await SemaphoreLock.TryTakeAsync(semaphore, TimeSpan.FromSeconds(1));

					if (lockInstance == null)
					{
						Debug.WriteLine("Short1 timed out.");
						short1TimedOutAt = Environment.TickCount;
					}
					else
					{
						using (lockInstance)
						{
							Debug.WriteLine("Short1 acquired lock.");
							await Task.Delay(50);
							Debug.WriteLine("Short1 completed.");
							short1CompletedAt = Environment.TickCount;
						}
					}
				});

				var shortTask2 = Task.Run(async () =>
				{
					Debug.WriteLine("Short2 entered.");
					await Task.Delay(500);
					Debug.WriteLine("Short2 starting work.");
					using (await SemaphoreLock.TakeAsync(semaphore))
					{
						Debug.WriteLine("Short2 acquired lock.");
						await Task.Delay(50);
						Debug.WriteLine("Short2 completed.");
						short2CompletedAt = Environment.TickCount;
					}
				});

				Task.WaitAll(longTask, shortTask1, shortTask2);

				Assert.False(short1CompletedAt.HasValue);
				Assert.True(short1TimedOutAt.HasValue);
				Assert.True(short2CompletedAt.HasValue);
				Assert.True(longCompletedAt.HasValue);
				Assert.True(short1TimedOutAt.Value < longCompletedAt.Value);
				Assert.True(short1TimedOutAt.Value < short2CompletedAt.Value);
				Assert.True(longCompletedAt.Value < short2CompletedAt.Value);
			}
		}

		[Fact]
		public void Exception_Unlocks()
		{
			using (var semaphore = new SemaphoreSlim(1))
			{
				try
				{
					using (SemaphoreLock.Take(semaphore))
					{
						throw new InvalidProgramException();
					}
				}
				catch (InvalidProgramException)
				{
				}

				// If this is false, the lock is still being held.
				Assert.True(semaphore.Wait(0));
			}
		}
	}
}