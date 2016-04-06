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

	public sealed class ExtensionsForTaskTests : TestClass
	{
		[Fact]
		public async Task WithAbandonment_AbandonsWhenCancelled()
		{
			// This needs to be 60 because in VSTS hosted agents, tests can run EXTEEEMELY slowly and irregularly.
			// At low values, you'll get a bunch of timeouts, so just keep it high to let VSTS do its thing.
			var sleepTime = TimeSpan.FromSeconds(60);
			var cancelTime = TimeSpan.FromMilliseconds(500);

			// We cancel very fast but the thread continues to sleep for quite a while longer.
			// Success means that we get a cancellation exception AND that it happens before the sleep finishes.
			var cts = new CancellationTokenSource(cancelTime);
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			await Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(sleepTime).WithAbandonment(cts.Token));

			stopwatch.Stop();

			Log.Default.Debug($"Elapsed {stopwatch.Elapsed.TotalSeconds:F1}s; sleep time was {sleepTime.TotalSeconds:F1}s.");

			Assert.True(stopwatch.Elapsed < sleepTime);
		}
	}
}