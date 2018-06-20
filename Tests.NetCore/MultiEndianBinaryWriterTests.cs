namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System.Linq;

    [TestClass]
    public sealed class MultiEndianBinaryWriterTests : BaseTestClass
    {
        private MultiEndianBinaryWriter _bigEndianWriter;
        private byte[] _bigEndianBuffer;
        private MultiEndianBinaryWriter _littleEndianWriter;
        private byte[] _littleEndianBuffer;

        public MultiEndianBinaryWriterTests()
        {
            Reset();
        }

        public void Reset()
        {
            _bigEndianBuffer = new byte[8];
            _littleEndianBuffer = new byte[8];

            _bigEndianWriter = new MultiEndianBinaryWriter(new MemoryStream(_bigEndianBuffer), ByteOrder.BigEndian);
            _littleEndianWriter = new MultiEndianBinaryWriter(new MemoryStream(_littleEndianBuffer), ByteOrder.LittleEndian);
        }

        [TestMethod]
        public void TestBinaryWriteMethods()
        {
            // We write all the stuff with both writers and compare.

            unchecked // For ease of copypaste.
            {
                _bigEndianWriter.Write((short)0x0102030405060708);
                _littleEndianWriter.Write((short)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(short));
                Reset();

                _bigEndianWriter.Write((ushort)0x0102030405060708);
                _littleEndianWriter.Write((ushort)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(ushort));
                Reset();

                _bigEndianWriter.Write((int)0x0102030405060708);
                _littleEndianWriter.Write((int)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(int));
                Reset();

                _bigEndianWriter.Write((uint)0x0102030405060708);
                _littleEndianWriter.Write((uint)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(uint));
                Reset();

                _bigEndianWriter.Write(0x0102030405060708);
                _littleEndianWriter.Write(0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(long));
                Reset();

                _bigEndianWriter.Write((ulong)0x0102030405060708);
                _littleEndianWriter.Write((ulong)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(ulong));
                Reset();

                _bigEndianWriter.Write((float)0x0102030405060708);
                _littleEndianWriter.Write((float)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(float));
                Reset();

                _bigEndianWriter.Write((double)0x0102030405060708);
                _littleEndianWriter.Write((double)0x0102030405060708);
                Verify(_bigEndianBuffer, _littleEndianBuffer, sizeof(double));
                Reset();
            }
        }

        private static void Verify(byte[] one, byte[] other, int dataSize)
        {
            var oneData = one.Take(dataSize).ToArray();
            var otherData = other.Take(dataSize).Reverse().ToArray();

            CollectionAssert.AreEqual(oneData, otherData);
        }
    }
}