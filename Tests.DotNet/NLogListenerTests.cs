namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Axinom.Toolkit.DotNet.NLog;
	using NLog;
	using Xunit;

	public sealed class NLogListenerTests
	{
		[Fact]
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

				Assert.Equal(5, memoryTarget.Logs.Count);
				Assert.Equal("Debug", memoryTarget.Logs[0]);
				Assert.Equal("Info", memoryTarget.Logs[1]);
				Assert.Equal("Warning", memoryTarget.Logs[2]);
				Assert.Equal("Error", memoryTarget.Logs[3]);
				Assert.Equal("Wtf", memoryTarget.Logs[4]);
			}
		}
	}
}