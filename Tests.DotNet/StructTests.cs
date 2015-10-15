namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Axinom.Toolkit;
	using Xunit;

	public class StructTests : TestClass
	{
		private struct Packet
		{
			public int A;
		}

		[Fact]
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

			Assert.Equal(packet, other);
		}

		[Fact]
		public void ByteOrderIsLittleEndian()
		{
			var stream = new MemoryStream(new byte[] { 1, 0, 0, 0 });

			var packet = Helpers.Struct.Read<Packet>(stream);

			Assert.Equal(1, packet.A);
		}

		[Fact]
		public void InvalidTypeIsRejected()
		{
			Assert.Throws<ArgumentException>(() => Helpers.Struct.Write(new StructTests(), new MemoryStream()));
		}
	}
}