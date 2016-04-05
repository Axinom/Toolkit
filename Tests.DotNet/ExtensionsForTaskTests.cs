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
	using Xunit.Abstractions;

	public sealed class ExtensionsForTaskTests : TestClass
	{
		private readonly ITestOutputHelper _output;

		public ExtensionsForTaskTests(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public async Task WithAbandonment_AbandonsWhenCancelled()
		{
			var sleepTime = TimeSpan.FromSeconds(10);
			var cancelTime = TimeSpan.FromMilliseconds(500);

			// We cancel very fast but the thread continues to sleep for quite a while longer.
			// Success means that we get a cancellation exception AND that it happens before the sleep finishes.
			var cts = new CancellationTokenSource(cancelTime);
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			await Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(sleepTime).WithAbandonment(cts.Token));

			stopwatch.Stop();

			_output.WriteLine($"Elapsed {stopwatch.Elapsed.TotalSeconds:F1}s; sleep time was {sleepTime.TotalSeconds:F1}s.");

			Assert.True(stopwatch.Elapsed < sleepTime);
		}
	}
}