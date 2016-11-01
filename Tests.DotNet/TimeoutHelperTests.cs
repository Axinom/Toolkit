using Axinom.Toolkit;
using System;
using Xunit;

namespace Tests
{
	public sealed class TimeoutHelperTests
	{
		[Fact]
		public void GetCancellationToken_WithNegativeTimeout_IsImmediatelyCancelled()
		{
			var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.FromSeconds(-1));

			Assert.True(cancel.IsCancellationRequested);
		}

		[Fact]
		public void GetCancellationToken_WithZeroTimeout_IsImmediatelyCancelled()
		{
			var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.Zero);

			Assert.True(cancel.IsCancellationRequested);
		}

		[Fact]
		public void GetCancellationToken_WithPositiveTimeout_IsNotImmediatelyCancelled()
		{
			var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.FromSeconds(11));

			Assert.False(cancel.IsCancellationRequested);
		}
	}
}
