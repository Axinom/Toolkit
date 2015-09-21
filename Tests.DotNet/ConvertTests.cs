namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class ConvertTests
	{
		private static readonly byte[] TestBytes = new byte[15001];
		private static readonly string TestString;

		static ConvertTests()
		{
			new Random(50).NextBytes(TestBytes);
			TestString = Helpers.Random.GetWords(5000, 5000);
		}

		[Test]
		public void Base32EncodeAndDecodeBytesMatch()
		{
			var encoded = Helpers.Convert.Base32EncodeBytes(TestBytes);
			var decoded = Helpers.Convert.Base32DecodeBytes(encoded);

			CollectionAssert.AreEqual(TestBytes, decoded);
		}

		[Test]
		public void Base32EncodeAndDecodeStringMatch()
		{
			var encoded = Helpers.Convert.Base32EncodeString(TestString);
			var decoded = Helpers.Convert.Base32DecodeString(encoded);

			Assert.AreEqual(TestString, decoded);
		}

		[Test]
		public void ByteArrayToHexString_WithEmptyByteArray_ReturnsEmptyString()
		{
			var hexString = Helpers.Convert.ByteArrayToHexString(new byte[0]);
			Assert.AreEqual(0, hexString.Length);
		}

		[Test]
		public void ByteArrayToHexString_ReturnsExpectedString()
		{
			var hexString = Helpers.Convert.ByteArrayToHexString(new byte[] { 0x01, 0x0A, 0xCC });
			StringAssert.AreEqualIgnoringCase("010acc", hexString);
		}

		[Test]
		public void ByteArrayToHexString_ThenHexStringToByteArray_ReturnsOriginalByteArray()
		{
			var bytes = new byte[] { 0x01, 0x0A, 0xCC };
			var hexString = Helpers.Convert.ByteArrayToHexString(bytes);
			var bytes2 = Helpers.Convert.HexStringToByteArray(hexString);

			CollectionAssert.AreEqual(bytes, bytes2);
		}

		[Test]
		public void HexStringToByteArray_WithLowercaseAndUppercase_ReturnsSameByteArrayForBoth()
		{
			const string hexString = "005f8adc";

			var bytes1 = Helpers.Convert.HexStringToByteArray(hexString.ToUpperInvariant());
			var bytes2 = Helpers.Convert.HexStringToByteArray(hexString.ToLowerInvariant());

			CollectionAssert.AreEqual(bytes1, bytes2);
		}

		[Test]
		public void HexStringToByteArray_WithOddNumberOfCharacters_ThrowsException()
		{
			Assert.Throws<ArgumentException>(() => Helpers.Convert.HexStringToByteArray("a"));
		}
	}
}