namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class ConvertTests
	{
		private static readonly byte[] TestBytes = new byte[15001];
		private static readonly string TestString;

		static ConvertTests()
		{
			new Random(50).NextBytes(TestBytes);
			TestString = Helpers.Random.GetWords(5000, 5000);
		}

		[Fact]
		public void Base32EncodeAndDecodeBytesMatch()
		{
			var encoded = Helpers.Convert.Base32EncodeBytes(TestBytes);
			var decoded = Helpers.Convert.Base32DecodeBytes(encoded);

			Assert.Equal(TestBytes, decoded);
		}

		[Fact]
		public void Base32EncodeAndDecodeStringMatch()
		{
			var encoded = Helpers.Convert.Base32EncodeString(TestString);
			var decoded = Helpers.Convert.Base32DecodeString(encoded);

			Assert.Equal(TestString, decoded);
		}

		[Fact]
		public void ByteArrayToHexString_WithEmptyByteArray_ReturnsEmptyString()
		{
			var hexString = Helpers.Convert.ByteArrayToHexString(new byte[0]);
			Assert.Equal(0, hexString.Length);
		}

		[Fact]
		public void ByteArrayToHexString_ReturnsExpectedString()
		{
			var hexString = Helpers.Convert.ByteArrayToHexString(new byte[] { 0x01, 0x0A, 0xCC });
			Assert.True("010acc".Equals(hexString, StringComparison.OrdinalIgnoreCase));
		}

		[Fact]
		public void ByteArrayToHexString_ThenHexStringToByteArray_ReturnsOriginalByteArray()
		{
			var bytes = new byte[] { 0x01, 0x0A, 0xCC };
			var hexString = Helpers.Convert.ByteArrayToHexString(bytes);
			var bytes2 = Helpers.Convert.HexStringToByteArray(hexString);

			Assert.Equal(bytes, bytes2);
		}

		[Fact]
		public void HexStringToByteArray_WithLowercaseAndUppercase_ReturnsSameByteArrayForBoth()
		{
			const string hexString = "005f8adc";

			var bytes1 = Helpers.Convert.HexStringToByteArray(hexString.ToUpperInvariant());
			var bytes2 = Helpers.Convert.HexStringToByteArray(hexString.ToLowerInvariant());

			Assert.Equal(bytes1, bytes2);
		}

		[Fact]
		public void HexStringToByteArray_WithOddNumberOfCharacters_ThrowsException()
		{
			Assert.Throws<ArgumentException>(() => Helpers.Convert.HexStringToByteArray("a"));
		}
	}
}