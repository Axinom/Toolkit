namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class ExtensionsForArrayTests
	{
		[Fact]
		public void ArrayContainsArray_WithNeedleLargerThanHaystack_ReturnsFalse()
		{
			var needle = new[] { 1, 2, 3, 4, 5 };
			var haystack = new[] { 1, 2, 3, 4 };

			Assert.False(haystack.ContainsArray(needle));
		}


		[Fact]
		public void ArrayContainsArray_WithEmptyNeedle_ReturnsFalse()
		{
			var needle = new int[0];
			var haystack = new[] { 1, 2, 3, 4 };

			Assert.False(haystack.ContainsArray(needle));
		}

		[Fact]
		public void ArrayContainsArray_WithNeedleInBeginning_ReturnsTrue()
		{
			var needle = new[] { 1, 2, 3 };
			var haystack = new[] { 1, 2, 3, 4 };

			Assert.True(haystack.ContainsArray(needle));
		}

		[Fact]
		public void ArrayContainsArray_WithNeedleInMiddle_ReturnsTrue()
		{
			var needle = new[] { 2, 3 };
			var haystack = new[] { 1, 2, 3, 4 };

			Assert.True(haystack.ContainsArray(needle));
		}

		[Fact]
		public void ArrayContainsArray_WithNeedleInEnd_ReturnsTrue()
		{
			var needle = new[] { 3, 4 };
			var haystack = new[] { 1, 2, 3, 4 };

			Assert.True(haystack.ContainsArray(needle));
		}

		[Fact]
		public void ArrayContainsArray_WithNeedleEqualsHaystack_ReturnsTrue()
		{
			var needle = new[] { 1, 2, 3, 4, 5 };
			var haystack = new[] { 1, 2, 3, 4, 5 };

			Assert.True(haystack.ContainsArray(needle));
		}
	}
}