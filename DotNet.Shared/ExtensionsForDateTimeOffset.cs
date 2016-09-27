namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class ExtensionsForDateTimeOffset
	{
		public static bool IsInRange(this DateTimeOffset date, DateTimeOffset minDate, DateTimeOffset maxDate)
		{
			return minDate <= date && date <= maxDate;
		}
	}
}