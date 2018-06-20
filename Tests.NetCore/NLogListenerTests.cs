namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog;

    [TestClass]
    public sealed class NLogListenerTests : BaseTestClass
    {
        [TestMethod]
        public void NLogListener_WritesLogEntriesToNLog()
        {
            using (var log = new Log())
            using (var loggerFactory = new LogFactory())
            {
                var memoryTarget = Helpers.NLog.SetupCapturingLoggingConfiguration(loggerFactory);
                var listener = new NLogListener(loggerFactory);

                log.RegisterListener(listener);

                log.Debug("Debug");
                log.Info("Info");
                log.Warning("Warning");
                log.Error("Error");
                log.Wtf("Wtf");

                Assert.AreEqual(5, memoryTarget.Logs.Count);
                Assert.AreEqual("Debug", memoryTarget.Logs[0]);
                Assert.AreEqual("Info", memoryTarget.Logs[1]);
                Assert.AreEqual("Warning", memoryTarget.Logs[2]);
                Assert.AreEqual("Error", memoryTarget.Logs[3]);
                Assert.AreEqual("Wtf", memoryTarget.Logs[4]);
            }
        }
    }
}