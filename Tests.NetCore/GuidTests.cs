namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public sealed class GuidTests : BaseTestClass
    {
        [TestMethod]
        public void ToBigEndianByteArray_ResultIsNotSameAsToByteArray()
        {
            var guid = new Guid("B9288A6A-AAE6-4905-8E59-B94C24AA7064");

            var littleEndian = guid.ToByteArray();
            var bigEndian = guid.ToBigEndianByteArray();

            Assert.AreNotEqual(bigEndian, littleEndian);
        }

        [TestMethod]
        public void ToBigEndianByteArray_ThenFromBigEndianByteArray_ReturnsOriginalGuid()
        {
            var guid = new Guid("B9288A6A-AAE6-4905-8E59-B94C24AA7064");

            var serialized = guid.ToBigEndianByteArray();
            var deserialized = Helpers.Guid.FromBigEndianByteArray(serialized);

            Assert.AreEqual(guid, deserialized);
        }
    }
}