namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class CertificateTests : TestClass
	{
		[Fact]
		public void CleanWindowsThumbprint_WithCleanThumbprint_IsNoOp()
		{
			var original = "54800ce83a711103d6d2f7f88509d0de5970052d";
			var cleaned = Helpers.Certificate.CleanWindowsThumbprint(original);

			Assert.Equal(original, cleaned);
		}

		[Fact]
		public void CleanWindowsThumbprint_WithWindowsThumbprint_CleansAsExpected()
		{
			// There is an annoying invisible character left of the 84.
			//              V---- here
			var original = "‎84 80 0c e8 3a 71 11 03 d6 d2 f7 f8 85 09 d0 de 59 70 05 2d";
			var cleaned = Helpers.Certificate.CleanWindowsThumbprint(original);

			Assert.Equal("84800ce83a711103d6d2f7f88509d0de5970052d", cleaned);
		}
	}
}