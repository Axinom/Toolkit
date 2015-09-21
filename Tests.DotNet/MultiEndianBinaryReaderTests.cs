namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class MultiEndianBinaryReaderTests
	{
		private static readonly byte[] TestData = { 1, 2, 3, 4, 5, 6, 7, 8 };
		private MultiEndianBinaryReader _reader;

		[SetUp]
		public void Reset()
		{
			_reader = new MultiEndianBinaryReader(new MemoryStream(TestData), ByteOrder.LittleEndian);
		}

		[Test]
		public void TestBinaryReadMethods()
		{
			// First, we read all in little-endian.
			var int16l = _reader.ReadInt16();
			Reset();
			var uint16l = _reader.ReadUInt16();
			Reset();
			var int32l = _reader.ReadInt32();
			Reset();
			var uint32l = _reader.ReadUInt32();
			Reset();
			var int64l = _reader.ReadInt64();
			Reset();
			var uint64l = _reader.ReadUInt64();
			Reset();
			var singlel = _reader.ReadSingle();
			Reset();
			var doublel = _reader.ReadDouble();
			Reset();

			// Then all in big-endian.
			_reader.ByteOrder = ByteOrder.BigEndian;
			var int16b = _reader.ReadInt16();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var uint16b = _reader.ReadUInt16();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var int32b = _reader.ReadInt32();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var uint32b = _reader.ReadUInt32();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var int64b = _reader.ReadInt64();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var uint64b = _reader.ReadUInt64();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var singleb = _reader.ReadSingle();
			Reset();
			_reader.ByteOrder = ByteOrder.BigEndian;
			var doubleb = _reader.ReadDouble();

			Assert.AreEqual(int16l, 0x0201);
			Assert.AreEqual(uint16l, 0x0201);
			Assert.AreEqual(int32l, 0x04030201);
			Assert.AreEqual(uint32l, 0x04030201);
			Assert.AreEqual(int64l, 0x0807060504030201);
			Assert.AreEqual(uint64l, 0x0807060504030201);

			Assert.AreEqual(int16b, 0x0102);
			Assert.AreEqual(uint16b, 0x0102);
			Assert.AreEqual(int32b, 0x01020304);
			Assert.AreEqual(uint32b, 0x01020304);
			Assert.AreEqual(int64b, 0x0102030405060708);
			Assert.AreEqual(uint64b, 0x0102030405060708);

			// We don't have direct casts for these (they convert, not memory cast), so let's just compare for inequality.
			Assert.AreNotEqual(singlel, singleb);
			Assert.AreNotEqual(doublel, doubleb);
		}

		[Test]
		[ExpectedException(typeof(EndOfStreamException))]
		public void EndOfStreamIsThrown()
		{
			var reader = new MultiEndianBinaryReader(new MemoryStream(new byte[3]), ByteOrder.LittleEndian);

			reader.ReadInt32();
		}
	}
}