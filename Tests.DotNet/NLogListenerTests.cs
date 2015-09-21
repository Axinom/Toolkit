namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Axinom.Toolkit.DotNet.NLog;
	using NLog;
	using NUnit.Framework;

	[TestFixture]
	public sealed class NLogListenerTests
	{
		[Test]
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