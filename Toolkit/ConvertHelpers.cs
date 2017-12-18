namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static partial class NetStandardHelpers
	{
		#region Base64 and base32 string helpers
		/// <summary>
		/// Decodes a base64-encoded string, given the encoding used by the encoded string in binary format (defaults to UTF-8).
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="base64"/> is null.</exception>
		public static string Base64DecodeString(this HelpersContainerClasses.Convert container, string base64, Encoding encoding = null)
		{
			Helpers.Argument.ValidateIsNotNull(base64, "base64");

			if (encoding == null)
				encoding = Encoding.UTF8;

			var bytes = Convert.FromBase64String(base64);

			return encoding.GetString(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Base64-encodes a string, given the encoding used by the string in binary format (defaults to UTF-8).
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
		public static string Base64EncodeString(this HelpersContainerClasses.Convert container, string text, Encoding encoding = null)
		{
			Helpers.Argument.ValidateIsNotNull(text, "text");

			if (encoding == null)
				encoding = Encoding.UTF8;

			var bytes = encoding.GetBytes(text);

			return Convert.ToBase64String(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Base32-encodes a string, given the encoding used by the string in binary format (defaults to UTF-8).
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
		public static string Base32EncodeString(this HelpersContainerClasses.Convert container, string text, Encoding encoding = null)
		{
			Helpers.Argument.ValidateIsNotNull(text, "text");

			if (encoding == null)
				encoding = Encoding.UTF8;

			var bytes = encoding.GetBytes(text);

			return Helpers.Convert.Base32EncodeBytes(bytes);
		}

		/// <summary>
		/// Decodes a base32-encoded string, given the encoding used by the encoded string in binary format (defaults to UTF-8).
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="base32"/> is null.</exception>
		public static string Base32DecodeString(this HelpersContainerClasses.Convert container, string base32, Encoding encoding = null)
		{
			Helpers.Argument.ValidateIsNotNull(base32, "base32");

			if (encoding == null)
				encoding = Encoding.UTF8;

			var bytes = Helpers.Convert.Base32DecodeBytes(base32);

			return encoding.GetString(bytes, 0, bytes.Length);
		}
        #endregion

        #region Base64url
        public static string ByteArrayToBase64Url(this HelpersContainerClasses.Convert container, byte[] bytes)
        {
            Helpers.Argument.ValidateIsNotNull(bytes, nameof(bytes));

            // We also remove padding because most usages of base64url do not want it.
            return Convert.ToBase64String(bytes).Replace('/', '_').Replace('+', '-').TrimEnd('=');
        }

        public static byte[] Base64UrlToByteArray(this HelpersContainerClasses.Convert container, string base64url)
        {
            Helpers.Argument.ValidateIsNotNull(base64url, nameof(base64url));

            // .NET implementation requires padding, so let's add it back if needed.
            var padding = new string('=', 4 - (base64url.Length % 4));

            return Convert.FromBase64String(base64url.Replace('_', '/').Replace('-', '+') + padding);
        }
        #endregion

        #region Base32
        /// <summary>
        /// This is not real base32. Rather, it usea a custom algorithm that does not have easily confusable characters.
        /// From http://www.atrevido.net/blog/PermaLink.aspx?guid=debdd47c-9d15-4a2f-a796-99b0449aa8af
        /// </summary>
        private const string Base32Alphabet = "QAZ2WSX3" + "EDC4RFV5" + "TGB6YHN7" + "UJM8K9LP";

		public static string Base32EncodeBytes(this HelpersContainerClasses.Convert container, byte[] bytes)
		{
			Helpers.Argument.ValidateIsNotNull(bytes, "bytes");

			var sb = new StringBuilder((int)Math.Ceiling(bytes.Length * 8.0 / 5));

			// WTF does hi mean?
			int hi = 5;
			int currentByte = 0;

			unchecked
			{
				while (currentByte < bytes.Length)
				{
					byte charIndex;

					// do we need to use the next byte?
					if (hi > 8)
					{
						// get the last piece from the current byte, shift it to the right
						// and increment the byte counter
						charIndex = (byte)(bytes[currentByte++] >> (hi - 5));
						if (currentByte != bytes.Length)
						{
							// if we are not at the end, get the first piece from
							// the next byte, clear it and shift it to the left
							charIndex = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | charIndex);
						}

						hi -= 3;
					}
					else if (hi == 8)
					{
						charIndex = (byte)(bytes[currentByte++] >> 3);
						hi -= 3;
					}
					else
					{
						// simply get the stuff from the current byte
						charIndex = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
						hi += 5;
					}

					sb.Append(Base32Alphabet[charIndex]);
				}
			}

			return sb.ToString();
		}

		public static byte[] Base32DecodeBytes(this HelpersContainerClasses.Convert container, string base32)
		{
			Helpers.Argument.ValidateIsNotNull(base32, "base32");

			// all UPPERCASE chars
			base32 = base32.ToUpper();

			if (base32.Any(c => !Base32Alphabet.Contains(c)))
				throw new ArgumentException("Input contains illegal characters.", "base32");

			int numBytes = base32.Length * 5 / 8;
			byte[] bytes = new Byte[numBytes];

			unchecked
			{
				if (base32.Length < 3)
				{
					bytes[0] = (byte)(Base32Alphabet.IndexOf(base32[0]) | Base32Alphabet.IndexOf(base32[1]) << 5);
					return bytes;
				}

				int bitBuffer = (Base32Alphabet.IndexOf(base32[0]) | Base32Alphabet.IndexOf(base32[1]) << 5);
				int bitsInBuffer = 10;
				int currentCharIndex = 2;

				for (int i = 0; i < bytes.Length; i++)
				{
					bytes[i] = (byte)bitBuffer;
					bitBuffer >>= 8;
					bitsInBuffer -= 8;
					while (bitsInBuffer < 8 && currentCharIndex < base32.Length)
					{
						bitBuffer |= Base32Alphabet.IndexOf(base32[currentCharIndex++]) << bitsInBuffer;
						bitsInBuffer += 5;
					}
				}
			}

			return bytes;
		}
		#endregion

		/// <summary>
		/// Converts a byte array to a hex string, with no delimiting characters anywhere.
		/// The returned hex string may use either uppercase or lowercase for the alphabetical characters.
		/// </summary>
		public static string ByteArrayToHexString(this HelpersContainerClasses.Convert container, byte[] bytes)
		{
			Helpers.Argument.ValidateIsNotNull(bytes, "bytes");

			var hex = BitConverter.ToString(bytes);
			return hex.Replace("-", "");
		}

		/// <summary>
		/// Converts a hex string into a byte array.
		/// </summary>
		/// <param name="hexString">
		/// A hex string with no delimiting characters (e.g. "aabbcc8811").
		/// Both uppercase and lowercase may be used.
		/// </param>
		public static byte[] HexStringToByteArray(this HelpersContainerClasses.Convert container, string hexString)
		{
			Helpers.Argument.ValidateIsNotNull(hexString, "hexString");

			if (hexString.Length % 2 != 0)
				throw new ArgumentException("The hex string must have an even number of characters.", "hexString");

			var bytes = new byte[hexString.Length / 2];
			for (var i = 0; i < hexString.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

			return bytes;
		}
	}
}