namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Windows.UI;
	using Windows.UI.Xaml.Data;
	using Windows.UI.Xaml.Media;

	/// <summary>
	/// Converts a Color or string containing a Color in [AA]RRGGBB format into a <see cref="SolidColorBrush"/>.
	/// Accepted input type and format is whatever is accepted by <see cref="ColorConverter"/>.
	/// </summary>
	/// <remarks>
	/// Essentially, this is equivalent to ColorConverter but just allows you to skip the step of defining a
	/// SolidColorBrush in XAML. It is provided for convenience, since solid color brushes are very commonly used.
	/// </remarks>
	public sealed class SolidColorBrushConverter : IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!targetType.IsAssignableFrom(typeof(SolidColorBrush)))
				throw new InvalidOperationException("SolidColorBrushConverter can only convert to SolidColorBrush.");

			if (value == null)
				return null;

			return new SolidColorBrush((Color)new ColorConverter().Convert(value, typeof(Color), null, language));
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}