namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using Windows.UI;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// Converts a string containing a Color in [AA]RRGGBB format into a Color.
	/// </summary>
	public sealed class ColorConverter : IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!targetType.IsAssignableFrom(typeof(Color)))
				throw new InvalidOperationException("ColorConverter can only convert to Color.");

			if (value == null)
				return null;

			if (!(value is Color) && !(value is string))
				throw new InvalidOperationException("Can only convert from Color or a string representation of Color.");

			if (value is string)
				value = ConvertToColor((string)value);

			return value;
		}

		internal static Color ConvertToColor(string colorString)
		{
			if (colorString.Length != 6 && colorString.Length != 8)
				throw new InvalidOperationException("Can only convert from color-strings of format [AA]RRGGBB.");

			byte a = 255;
			int offset = 0;

			if (colorString.Length == 8)
			{
				offset = 2;
				a = byte.Parse(colorString.Substring(0, 2), NumberStyles.HexNumber);
			}

			byte r = byte.Parse(colorString.Substring(offset + 0, 2), NumberStyles.HexNumber);
			byte g = byte.Parse(colorString.Substring(offset + 2, 2), NumberStyles.HexNumber);
			byte b = byte.Parse(colorString.Substring(offset + 4, 2), NumberStyles.HexNumber);

			return Color.FromArgb(a, r, g, b);
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates whether a string is a valid color string, convertible by the converter.
		/// </summary>
		public static bool IsValidColor(string colorString)
		{
			if (colorString == null)
				throw new ArgumentNullException(nameof(colorString));

			try
			{
				ConvertToColor(colorString);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}