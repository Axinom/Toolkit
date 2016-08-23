namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static partial class CoreHelpers
	{
		private static readonly Guid AsfHeaderId = new Guid("75B22630-668E-11CF-A6D9-00AA0062CE6C");

		/// <summary>
		/// Checks whether the provided streams contains a potentially valid ASF file.
		/// This does not perform any in-depth checks, just verifies that the proper file header exists.
		/// </summary>
		public static bool IsPotentiallyValidAsfFile(this HelpersContainerClasses.Asf container, Stream file)
		{
			Helpers.Argument.ValidateIsNotNull(file, "file");

			if (file.Length < 16)
				return false;

			byte[] extractedIdBytes = new byte[16];
			file.Read(extractedIdBytes, 0, 16);

			Guid extractedId = new Guid(extractedIdBytes);

			return extractedId == AsfHeaderId;
		}
	}
}