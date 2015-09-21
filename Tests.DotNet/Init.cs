namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[SetUpFixture]
	public sealed class Init
	{
		[SetUp]
		public void Start()
		{
			Log.Default.RegisterListener(new TraceLogListener());
		}
	}
}