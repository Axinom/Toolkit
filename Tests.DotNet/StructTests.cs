namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public class StructTests
	{
		// Some shared variables here to avoid re-declaring in every test.
		private MemoryStream _stream;
		private Packet _packet;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
			_packet = new Packet
			{
				A = 50
			};
		}

		private struct Packet
		{
			public int A;
		}

		[Test]
		public void BasicReadWriteSucceeds()
		{
			Helpers.Struct.Write(_packet, _stream);
			_stream.Position = 0;

			var other = Helpers.Struct.Read<Packet>(_stream);

			Assert.AreEqual(_packet, other);
		}

		[Test]
		public void ByteOrderIsLittleEndian()
		{
			var stream = new MemoryStream(new byte[] { 1, 0, 0, 0 });

			var packet = Helpers.Struct.Read<Packet>(stream);

			Assert.AreEqual(1, packet.A);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void InvalidTypeIsRejected()
		{
			Helpers.Struct.Write(new StructTests(), _stream);
		}
	}
}