﻿namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static partial class UwpHelpers
	{
		/// <summary>
		/// Gets whether the code is running on a Microsoft operating system that can be assumed to resemble Windows.
		/// </summary>
		public static bool IsMicrosoftOperatingSystem(this HelpersContainerClasses.Environment container)
		{
			// For parity with DotNet.
			return true;
		}

		/// <summary>
		/// Gets whether the code is running on a non-Microsoft operating system that can be assumed to resemble Linux.
		/// </summary>
		public static bool IsNonMicrosoftOperatingSystem(this HelpersContainerClasses.Environment container)
		{
			// For parity with DotNet.
			return false;
		}
	}
}