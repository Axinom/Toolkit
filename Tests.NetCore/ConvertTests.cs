namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public sealed class ConvertTests : TestClass
    {
        private static readonly byte[] TestBytes = new byte[15001];
        private static readonly string TestString;

        static ConvertTests()
        {
            new Random(50).NextBytes(TestBytes);
            TestString = Helpers.Random.GetWords(5000, 5000);
        }

        [TestMethod]
        public void Base32EncodeAndDecodeBytesMatch()
        {
            var encoded = Helpers.Convert.Base32EncodeBytes(TestBytes);
            var decoded = Helpers.Convert.Base32DecodeBytes(encoded);

            CollectionAssert.AreEqual(TestBytes, decoded);
        }

        [TestMethod]
        public void Base32EncodeAndDecodeStringMatch()
        {
            var encoded = Helpers.Convert.Base32EncodeString(TestString);
            var decoded = Helpers.Convert.Base32DecodeString(encoded);

            Assert.AreEqual(TestString, decoded);
        }

        [TestMethod]
        public void ByteArrayToHexString_WithEmptyByteArray_ReturnsEmptyString()
        {
            var hexString = Helpers.Convert.ByteArrayToHexString(new byte[0]);
            Assert.AreEqual(0, hexString.Length);
        }

        [TestMethod]
        public void ByteArrayToHexString_ReturnsExpectedString()
        {
            var hexString = Helpers.Convert.ByteArrayToHexString(new byte[] { 0x01, 0x0A, 0xCC });
            Assert.IsTrue("010acc".Equals(hexString, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ByteArrayToHexString_ThenHexStringToByteArray_ReturnsOriginalByteArray()
        {
            var bytes = new byte[] { 0x01, 0x0A, 0xCC };
            var hexString = Helpers.Convert.ByteArrayToHexString(bytes);
            var bytes2 = Helpers.Convert.HexStringToByteArray(hexString);

            CollectionAssert.AreEqual(bytes, bytes2);
        }

        [TestMethod]
        public void HexStringToByteArray_WithLowercaseAndUppercase_ReturnsSameByteArrayForBoth()
        {
            const string hexString = "005f8adc";

            var bytes1 = Helpers.Convert.HexStringToByteArray(hexString.ToUpperInvariant());
            var bytes2 = Helpers.Convert.HexStringToByteArray(hexString.ToLowerInvariant());

            CollectionAssert.AreEqual(bytes1, bytes2);
        }

        [TestMethod]
        public void HexStringToByteArray_WithOddNumberOfCharacters_ThrowsException()
        {
            Assert.ThrowsException<ArgumentException>(() => Helpers.Convert.HexStringToByteArray("a"));
        }

        [TestMethod]
        public void ByteArrayToBase64Url_RoundTrip_ReturnsSameBytes()
        {
            var encoded = Helpers.Convert.ByteArrayToBase64Url(TestBytes);
            var decoded = Helpers.Convert.Base64UrlToByteArray(encoded);

            CollectionAssert.AreEqual(TestBytes, decoded);
        }

        [TestMethod]
        public void ByteArrayToBase64Url_ReturnsNoPadding()
        {
            for (int i = 0; i < 10; i++)
            {
                var data = new byte[i];

                Assert.IsFalse(Helpers.Convert.ByteArrayToBase64Url(data).Contains("="));
            }
        }
    }
}