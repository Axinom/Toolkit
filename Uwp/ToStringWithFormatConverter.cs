namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// Executes an object.ToString(string) call on the value, passing a custom format string as the parameter.
	/// </summary>
	/// <remarks>
	/// Actually implemented via string.Format() because .ToString(string) is apparently missing on some types in Release build!
	/// System.Double, for example. Go figure. Anyway, it should work fine like this, as well.
	/// </remarks>
	public sealed class ToStringWithFormatConverter : IValueConverter
	{
		public string FormatString { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			if (FormatString == null)
				return value.ToString();

			try
			{
				return string.Format("{0:" + FormatString + "}", value);
			}
			catch
			{
				return value.ToString();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}