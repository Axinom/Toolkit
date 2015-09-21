namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// Converts any object into its Helpers.Debug.ToDebugString() representation.
	/// </summary>
	public sealed class DebugConverter : IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (targetType != typeof(string))
				throw new InvalidOperationException("DebugConverter can only convert to String.");

			if (value == null)
				return "null";

			return Helpers.Debug.ToDebugString(value);
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}