namespace Axinom.Toolkit
{
    using System.IO;
    using System.Runtime.InteropServices;

    public static partial class NetStandardHelpers
    {
        /// <summary>
        /// Writes the marshalled form of the structure to a buffer.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a marshallable type.</exception>
        /// <exception cref="IOException">Thrown if something goes wrong when writing the structure.</exception>
        public static byte[] Write<T>(this HelpersContainerClasses.Struct container, T value)
        {
            Helpers.Argument.ValidateIsNotNull(value, "value");

            int structSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structSize];

            // We create a pinned native handle, to allow this memory block to be used.
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return buffer;
        }

        /// <summary>
        /// Reads the marshalled form of a structure from the specified buffer.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if buffer is null.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a marshallable type.</exception>
        /// <exception cref="IOException">Thrown if something goes wrong when reading the structure.</exception>
        public static T Read<T>(this HelpersContainerClasses.Struct container, byte[] buffer)
        {
            Helpers.Argument.ValidateIsNotNull(buffer, "buffer");

            int structSize = Marshal.SizeOf(typeof(T));

            if (buffer.Length < structSize)
                throw new IOException(string.Format("Not enough bytes in buffer to deserialize {0}. Expected at least {1}, actual {2}.", typeof(T).FullName, structSize, buffer.Length));

            // We create a pinned native handle, to allow this memory block to be used.
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Writes the marshalled form of the structure to the specified stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if stream is null.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a marshallable type.</exception>
        /// <exception cref="IOException">Thrown if something goes wrong when writing the structure.</exception>
        public static void Write<T>(this HelpersContainerClasses.Struct container, T value, Stream stream)
        {
            Helpers.Argument.ValidateIsNotNull(stream, "stream");
            Helpers.Argument.ValidateIsNotNull(value, "value");

            var buffer = Helpers.Struct.Write(value);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Reads the marshalled form of a structure from the specified stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if stream is null.</exception>
        /// <exception cref="ArgumentException">Thrown if T is not a marshallable type.</exception>
        /// <exception cref="IOException">Thrown if something goes wrong when reading the structure.</exception>
        public static T Read<T>(this HelpersContainerClasses.Struct container, Stream stream)
        {
            Helpers.Argument.ValidateIsNotNull(stream, "stream");

            long bytesLeft = stream.Length - stream.Position;
            int structSize = Marshal.SizeOf(typeof(T));

            if (bytesLeft < structSize)
                throw new IOException(string.Format("Not enough bytes left in stream to deserialize {0}. Expected at least {1}, actual {2}.", typeof(T).FullName, structSize, bytesLeft));

            byte[] buffer = new byte[structSize];

            stream.ReadAndVerify(buffer, 0, buffer.Length);

            return Helpers.Struct.Read<T>(buffer);
        }
    }
}