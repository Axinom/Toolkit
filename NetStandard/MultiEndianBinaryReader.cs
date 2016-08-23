namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Binary reader with variable byte endianness.
	/// Note that text reading does not support big-endian byte order. This is for binary only.
	/// </summary>
	public class MultiEndianBinaryReader : BinaryReader
	{
		/// <summary>
		/// Gets or sets the byte order the reader uses for its operations.
		/// </summary>
		public ByteOrder ByteOrder { get; set; }

		/// <summary>
		/// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
		/// </summary>
		/// <returns>
		/// A 2-byte signed integer read from the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override short ReadInt16()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadInt16();

			return BitConverter.ToInt16(this.ReadBytesAndVerify(sizeof(Int16)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the
		/// stream by two bytes.
		/// </summary>
		/// <returns>
		/// A 2-byte unsigned integer read from this stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override ushort ReadUInt16()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadUInt16();

			return BitConverter.ToUInt16(this.ReadBytesAndVerify(sizeof(UInt16)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>
		/// A 4-byte signed integer read from the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override int ReadInt32()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadInt32();

			return BitConverter.ToInt32(this.ReadBytesAndVerify(sizeof(Int32)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
		/// </summary>
		/// <returns>
		/// A 4-byte unsigned integer read from this stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override uint ReadUInt32()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadUInt32();

			return BitConverter.ToUInt32(this.ReadBytesAndVerify(sizeof(UInt32)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>
		/// An 8-byte signed integer read from the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override long ReadInt64()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadInt64();

			return BitConverter.ToInt64(this.ReadBytesAndVerify(sizeof(Int64)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
		/// </summary>
		/// <returns>
		/// An 8-byte unsigned integer read from this stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		public override ulong ReadUInt64()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadUInt64();

			return BitConverter.ToUInt64(this.ReadBytesAndVerify(sizeof(UInt64)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four
		/// bytes.
		/// </summary>
		/// <returns>
		/// A 4-byte floating point value read from the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override float ReadSingle()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadSingle();

			return BitConverter.ToSingle(this.ReadBytesAndVerify(sizeof(Single)).Reverse().ToArray(), 0);
		}

		/// <summary>
		/// Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight
		/// bytes.
		/// </summary>
		/// <returns>
		/// An 8-byte floating point value read from the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached. </exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override double ReadDouble()
		{
			if (ByteOrder == ByteOrder.LittleEndian)
				return base.ReadDouble();

			return BitConverter.ToDouble(this.ReadBytesAndVerify(sizeof(Double)).Reverse().ToArray(), 0);
		}

		#region Initialization
		public MultiEndianBinaryReader(Stream input, ByteOrder byteOrder)
			: base(input)
		{
			if (!BitConverter.IsLittleEndian)
				throw new InvalidOperationException("This class is only designed for little endian machines and will almost certainly not function correctly on big endian machines.");

			ByteOrder = byteOrder;
		}

		public MultiEndianBinaryReader(Stream input, Encoding encoding, ByteOrder byteOrder)
			: base(input, encoding)
		{
			if (!BitConverter.IsLittleEndian)
				throw new InvalidOperationException("This class is only designed for little endian machines and will almost certainly not function correctly on big endian machines.");

			ByteOrder = byteOrder;
		}
		#endregion
	}
}