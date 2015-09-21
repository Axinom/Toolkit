namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;

	public static partial class DotNetHelpers
	{
		public static void TryReleaseArray<T>(this HelpersContainerClasses.Com container, ref T[] array)
			where T : class
		{
			if (array == null)
				return;

			foreach (var item in array)
			{
				T local = item;
				Helpers.Com.TryRelease(ref local);
			}

			array = null;
		}

		public static void TryRelease<T>(this HelpersContainerClasses.Com container, ref T comObject)
			where T : class
		{
			if (comObject == null)
				return;

			Marshal.ReleaseComObject(comObject);
			comObject = null;
		}
	}
}