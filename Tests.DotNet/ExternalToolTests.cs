namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class ExternalToolTests : TestClass
	{
		private static readonly TimeSpan ExecuteTimeout = TimeSpan.FromSeconds(5);

		[Fact]
		public void ExternalTool_StandardOutputIsCaptured()
		{
			const string canary = "146adgha4";
			var result = ExternalTool.Execute(TestData.CommandHandler, TestData.MakeCommandString(string.Format("echo {0}", canary)), ExecuteTimeout);

			Assert.Equal(0, result.ExitCode);
			Assert.Contains(canary, result.StandardOutput);
		}

		[Fact]
		public void ExternalTool_ConsumingResultWithStandardErrorOutput_ThrowsException()
		{
			const string canary = "6h4sb6455t";

			// Just echo something to stderr.
			Assert.Throws<EnvironmentException>(() => ExternalTool.Execute(TestData.CommandHandler, TestData.MakeCommandString(string.Format("echo {0} 1>&2", canary)), ExecuteTimeout));
		}

		[Fact]
		public void ExternalTool_ConsumingResultWithStandardErrorOutputButWithHeuristicDisabled_DoesNotThrowException()
		{
			const string canary = "6h4sb6455t";

			new ExternalTool
			{
				ExecutablePath = TestData.CommandHandler,
				// Just echo something to stderr.
				Arguments = TestData.MakeCommandString(string.Format("echo {0} 1>&2", canary)),
				ResultHeuristics = new ExternalToolResultHeuristics
				{
					StandardErrorIsNotError = true
				}
			}.Execute(ExecuteTimeout);
		}

		[Fact]
		public void ExternalTool_StandardErrorIsCaptured()
		{
			const string canary = "26usgnffff";

			// Need to do this the long way around to avoid consuming the result.
			var result = new ExternalTool
			{
				ExecutablePath = TestData.CommandHandler,
				Arguments = TestData.MakeCommandString(canary) // Nonsense command, should result in error.
			}.Start().GetResult(ExecuteTimeout);

			// Just for debug info.
			result.ForwardOutputs();

			Assert.NotEqual(0, result.ExitCode);
			Assert.Contains(canary, result.StandardError);
		}

		[Fact]
		public void ExternalTool_ConsumingResultWithNonSuccessfulExitCode_ThrowsException()
		{
			const string canary = "fgujw456hnt";

			// Nonsense command, should result in error.
			Assert.Throws<EnvironmentException>(() => ExternalTool.Execute(TestData.CommandHandler, TestData.MakeCommandString(canary), ExecuteTimeout));
		}

		[Fact]
		public void ExternalTool_WithStandardOutputAndStandardErrorContents_BothOutputStreamsAreWrittenToFile()
		{
			const string canary1 = "se5sm85";
			const string canary2 = "ze5vb75";

			var outputFile = Path.GetTempFileName();

			// Need to do this the long way around to avoid consuming the result.
			var result = new ExternalTool
			{
				ExecutablePath = TestData.CommandHandler,
				Arguments = TestData.MakeCommandString(string.Format("echo {0} & {1}", canary1, canary2)),
				OutputFilePath = outputFile
			}.Start().GetResult(ExecuteTimeout);

			// Just for debug info.
			result.ForwardOutputs();

			try
			{
				Assert.NotEqual(0, result.ExitCode);
				Assert.Contains(canary1, result.StandardOutput);
				Assert.Contains(canary2, result.StandardError);

				var outputFileContents = File.ReadAllText(outputFile);
				Assert.Contains(canary1, outputFileContents);
				Assert.Contains(canary2, outputFileContents);
			}
			finally
			{
				File.Delete(outputFile);
			}
		}

		[Fact]
		public void ExternalTool_WithTimeout_WritesBothOutputStreamsToFile()
		{
			const string canary1 = "eb6b46bu6";
			const string canary2 = "dsrt7n46n8";

			var outputFile = Path.GetTempFileName();

			// Need to do this the long way around to avoid consuming the result.
			var instance = new ExternalTool
			{
				ExecutablePath = TestData.CommandHandler,
				Arguments = TestData.MakeCommandString(string.Format("echo {0} & {1} & {2} 10", canary1, canary2, TestData.SleepCommandName)),
				OutputFilePath = outputFile
			}.Start();

			try
			{
				// Should be long enough that the echo succeeds but short enough that the timeout does not.
				instance.GetResult(TimeSpan.FromSeconds(2));

				throw new Exception("Should have timed out before reaching this point!");
			}
			catch (TimeoutException)
			{
				Debug.WriteLine("Expected timeout occurred.");
			}

			try
			{
				var outputFileContents = File.ReadAllText(outputFile);
				Assert.Contains(canary1, outputFileContents);
				Assert.Contains(canary2, outputFileContents);
			}
			finally
			{
				File.Delete(outputFile);
			}
		}

		[Fact]
		public void ExternalTool_WithCrashInTool_DoesNotHang()
		{
			using (var package = new EmbeddedPackage(Assembly.GetExecutingAssembly(), "Tests.TestData.CrashingTool", "crypt.xml", "js.dll", "libgpac.dll", "mp4box.exe", "ssleay32.dll", "z.mp4"))
			{
				var mp4BoxPath = Path.Combine(package.Path, "mp4box.exe");
				var cryptXmlPath = Path.Combine(package.Path, "crypt.xml");
				var inputFilePath = Path.Combine(package.Path, "z.mp4");

				using (var workingDirectory = new TemporaryDirectory())
				{
					var outputFilePath = Path.Combine(workingDirectory.Path, "out.mp4");

					// The tool in the package should crash essentially immediately, so we detect whether
					// a timeout occurs (bad) or whether the tool execution fails (good) or whether the tool succeeds (wtf).
					Assert.Throws<EnvironmentException>(() =>
						new ExternalTool
						{
							ExecutablePath = mp4BoxPath,
							Arguments = $"-noprog -crypt \"{cryptXmlPath}\" \"{inputFilePath}\" -out \"{outputFilePath}\"",
							ResultHeuristics = new ExternalToolResultHeuristics
							{
								StandardErrorIsNotError = true
							}
						}.Execute(TimeSpan.FromSeconds(5)));
				}
			}
		}

		private static EmbeddedPackage GetEchoPackage()
		{
			return new EmbeddedPackage(Assembly.GetExecutingAssembly(), "Tests.TestData", "Tests.Echo.exe", "Tests.Echo.pdb");
		}

		[Fact]
		public void ExternalTool_WithCustomInputProvider_UsesCustomInput()
		{
			var canary = Encoding.UTF8.GetBytes("sat9pyba8m5yiae5 hya");

			using (var package = GetEchoPackage())
			{
				var result = new ExternalTool
				{
					ExecutablePath = Path.Combine(package.Path, "Tests.Echo.exe"),
					Arguments = $"--echo {canary.Length}",
					StandardInputProvider = s =>
					{
						s.Write(canary, 0, canary.Length);
						s.Flush();
					}
				}.Execute(TimeSpan.FromSeconds(3600));

				Assert.Equal(canary, Encoding.UTF8.GetBytes(result.StandardOutput));
			}
		}

		[Fact]
		public void ExternalTool_WithCustomInputProviderUsingSmallBlockSize_UsesCustomInput()
		{
			var canary = Encoding.UTF8.GetBytes("sat9pyba8m5yiae5 hya");

			using (var package = GetEchoPackage())
			{
				var result = new ExternalTool
				{
					ExecutablePath = Path.Combine(package.Path, "Tests.Echo.exe"),
					Arguments = $"--echo {canary.Length} --blocksize 2",
					StandardInputProvider = s =>
					{
						s.Write(canary, 0, canary.Length);
						s.Flush();
					}
				}.Execute(TimeSpan.FromSeconds(5));

				Assert.Equal(canary, Encoding.UTF8.GetBytes(result.StandardOutput));
			}
		}

		// NOTE: This can fail to copy the stream in time if the system is under heavy load.
		[Fact]
		public void ExternalTool_WithLargeBinaryCustomInputAndOutput_RoundTripSucceeds()
		{
			var data = new byte[1024 * 1024];
			new Random().NextBytes(data);

			byte[] readData = null;

			using (var package = GetEchoPackage())
			{
				new ExternalTool
				{
					ExecutablePath = Path.Combine(package.Path, "Tests.Echo.exe"),
					Arguments = $"--echo {data.Length}",
					StandardInputProvider = s =>
					{
						s.Write(data, 0, data.Length);
						s.Flush();
					},
					StandardOutputConsumer = s =>
					{
						var readBuffer = new byte[1024];

						using (var buffer = new MemoryStream(data.Length))
						{
							while (true)
							{
								Log.Default.Debug("Reading more bytes.");

								var bytes = s.Read(readBuffer, 0, readBuffer.Length);

								Log.Default.Debug("Read {0} bytes.", bytes);

								if (bytes == 0)
								{
									// EOS!
									readData = buffer.ToArray();
									return;
								}

								buffer.Write(readBuffer, 0, bytes);
							}
						}
					}
				}.Execute(TimeSpan.FromSeconds(15));

				Assert.Equal(data, readData);
			}
		}

		internal static class TestData
		{
			/// <summary>
			/// Executable name of the operating system's basic command handler or text-mode shell.
			/// </summary>
			public static string CommandHandler
			{
				get
				{
					if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
						return "sh";
					else
						return "cmd.exe";
				}
			}

			/// <summary>
			/// Gets the string you need to give to the command handler in order to make it execute a command of some sort.
			/// No escaping is done so please do not use special characters or you'll be sorry!
			/// </summary>
			public static string MakeCommandString(string command)
			{
				if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
					return string.Format("-c \"{0}\"", command);
				else
					return string.Format("/c {0}", command);
			}

			/// <summary>
			/// Gets the name of the sleep command. It accepts one argument, which is the number of seconds to sleep for.
			/// </summary>
			public static string SleepCommandName
			{
				get
				{
					if (Helpers.Environment.IsNonMicrosoftOperatingSystem())
						return "sleep";
					else
						return "timeout";
				}
			}
		}
	}
}