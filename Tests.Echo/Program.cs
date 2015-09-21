namespace Tests.Echo
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Mono.Options;

	internal class Program
	{
		private static int Main(string[] args)
		{
			// This tool echoes back the content provided via stdin and then exits.
			// For testing ExternalTool stdin/stdout functionality.

			int bytesToEcho = 0;
			int blockSize = 1024;

			var options = new OptionSet
			{
				{ "echo=", (int val) => bytesToEcho = val },
				{ "blockSize=", (int val) => blockSize = val }
			};

			options.Parse(args);

			var stdin = Console.OpenStandardInput();
			var stdout = Console.OpenStandardOutput();

			var block = new byte[blockSize];

			while (bytesToEcho > 0)
			{
				var bytes = stdin.Read(block, 0, block.Length);

				if (bytes == 0)
				{
					// EOS?? We have not reached our count yet! Error!
					return -1;
				}

				stdout.Write(block, 0, bytes);
				bytesToEcho -= bytes;
			}

			return 0;
		}
	}
}