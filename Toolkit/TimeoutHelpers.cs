using System;
using System.Threading;

namespace Axinom.Toolkit
{
	public static partial class NetStandardHelpers
	{
		private static readonly CancellationToken CancelledAlready = new CancellationToken(true);

		/// <summary>
		/// Gets a cancellation token for a finite positive timeout.
		/// If the timeout time span is nonpositive, the returned cancellation token is already signaled.
		/// </summary>
		public static CancellationToken GetCancellationToken(this HelpersContainerClasses.Timeout container, TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				return CancelledAlready;

			return new CancellationTokenSource(timeout).Token;
		}
	}
}
