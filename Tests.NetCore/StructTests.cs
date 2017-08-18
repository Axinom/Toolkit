namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public class StructTests : TestClass
    {
        private struct Packet
        {
            public int A;
        }

        [TestMethod]
        public void BasicReadWriteSucceeds()
        {
            var stream = new MemoryStream();
            var packet = new Packet
            {
                A = 50
            };

            Helpers.Struct.Write(packet, stream);
            stream.Position = 0;

            var other = Helpers.Struct.Read<Packet>(stream);

            Assert.AreEqual(packet, other);
        }

        [TestMethod]
        public void ByteOrderIsLittleEndian()
        {
            var stream = new MemoryStream(new byte[] { 1, 0, 0, 0 });

            var packet = Helpers.Struct.Read<Packet>(stream);

            Assert.AreEqual(1, packet.A);
        }

        [TestMethod]
        public void InvalidTypeIsRejected()
        {
            Assert.ThrowsException<ArgumentException>(() => Helpers.Struct.Write(new StructTests(), new MemoryStream()));
        }
    }
}