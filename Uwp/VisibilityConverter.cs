namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// A value converter that converts values into equivalent Visibility values.
	/// 
	/// <list type="bullet">
	///		<item>For Boolean values, this maps true/false into Visible/Collapsed.</item>
	///		<item>For String values, this maps null and String.Empty into Collapsed.</item>
	///		<item>For collection values, this maps empty collections into Collapsed.</item>
	///		<item>For other values, this maps null into Collapsed.</item>
	/// </list>
	/// 
	/// <para>If the ConverterParameter is set to Inverse, then Visible is returned in place of Collapsed, and vice-versa.</para>
	/// </summary>
	public sealed class VisibilityConverter : IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (targetType != typeof(Visibility))
			{
				throw new ArgumentOutOfRangeException(nameof(targetType), "VisibilityConverter can only convert to Visibility");
			}

			Visibility visibility = Visibility.Visible;

			if (value == null)
			{
				visibility = Visibility.Collapsed;
			}
			else if (value is bool)
			{
				visibility = (bool)value ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (value is string)
			{
				visibility = String.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
			}
			else if (value is IEnumerable)
			{
				IEnumerable enumerable = (IEnumerable)value;
				if (enumerable.GetEnumerator().MoveNext() == false)
				{
					visibility = Visibility.Collapsed;
				}
			}

			if ((parameter is string) &&
			    (String.Compare((string)parameter, "Inverse", StringComparison.OrdinalIgnoreCase) == 0))
			{
				visibility = (visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
			}

			return visibility;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Visibility))
			{
				throw new ArgumentOutOfRangeException(nameof(value), "VisibilityConverter can only convert from Visibility");
			}

			if (targetType == typeof(bool))
			{
				return ((Visibility)value == Visibility.Visible) ? true : false;
			}

			throw new ArgumentOutOfRangeException(nameof(targetType), "VisibilityConverter can only convert to Boolean");
		}
	}
}