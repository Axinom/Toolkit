namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;

	/// <summary>
	/// Come on xunit, what do you force us to do. No global initialization built-in.
	/// </summary>
	public abstract class TestClass
	{
		protected TestClass()
		{
			RegisterListeners();
		}

		private static bool _registered;

		private static void RegisterListeners()
		{
			if (_registered)
				return;

			_registered = true;

			// Write output to temp file because for whatever reason, ReSharper does not show output via xunit.
			var filename = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff.log");
			var path = Path.Combine(Path.GetTempPath(), filename);
			var stream = File.Create(path);

			Log.Default.RegisterListener(new StreamWriterLogListener(new StreamWriter(stream)
			{
				AutoFlush = true
			}));
		}
	}
}