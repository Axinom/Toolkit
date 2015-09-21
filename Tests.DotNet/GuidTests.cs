namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class GuidTests
	{
		[Test]
		public void ToBigEndianByteArray_ResultIsNotSameAsToByteArray()
		{
			var guid = new Guid("B9288A6A-AAE6-4905-8E59-B94C24AA7064");

			var littleEndian = guid.ToByteArray();
			var bigEndian = guid.ToBigEndianByteArray();

			CollectionAssert.AreNotEqual(bigEndian, littleEndian);
		}

		[Test]
		public void ToBigEndianByteArray_ThenFromBigEndianByteArray_ReturnsOriginalGuid()
		{
			var guid = new Guid("B9288A6A-AAE6-4905-8E59-B94C24AA7064");

			var serialized = guid.ToBigEndianByteArray();
			var deserialized = Helpers.Guid.FromBigEndianByteArray(serialized);

			Assert.AreEqual(guid, deserialized);
		}
	}
}