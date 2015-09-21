namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class EnvironmentTests
	{
		[Test]
		public void IsMicrosoftOperatingSystem_IsOppositeOfIsNonMicrosoftOperatingSystem()
		{
			Assert.AreNotEqual(Helpers.Environment.IsNonMicrosoftOperatingSystem(), Helpers.Environment.IsMicrosoftOperatingSystem());
		}
	}
}