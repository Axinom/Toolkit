namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class EnvironmentTests
	{
		[Fact]
		public void IsMicrosoftOperatingSystem_IsOppositeOfIsNonMicrosoftOperatingSystem()
		{
			Assert.NotEqual(Helpers.Environment.IsNonMicrosoftOperatingSystem(), Helpers.Environment.IsMicrosoftOperatingSystem());
		}
	}
}