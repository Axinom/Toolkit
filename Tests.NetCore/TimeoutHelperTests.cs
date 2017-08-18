using Axinom.Toolkit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public sealed class TimeoutHelperTests
    {
        [TestMethod]
        public void GetCancellationToken_WithNegativeTimeout_IsImmediatelyCancelled()
        {
            var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.FromSeconds(-1));

            Assert.IsTrue(cancel.IsCancellationRequested);
        }

        [TestMethod]
        public void GetCancellationToken_WithZeroTimeout_IsImmediatelyCancelled()
        {
            var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.Zero);

            Assert.IsTrue(cancel.IsCancellationRequested);
        }

        [TestMethod]
        public void GetCancellationToken_WithPositiveTimeout_IsNotImmediatelyCancelled()
        {
            var cancel = Helpers.Timeout.GetCancellationToken(TimeSpan.FromSeconds(11));

            Assert.IsFalse(cancel.IsCancellationRequested);
        }
    }
}
