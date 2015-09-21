namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml.Data;

	/// <summary>
	/// A value converter that converts values into equivalent 0 or 1 opacity values.
	/// 
	/// <list type="bullet">
	///		<item>For Boolean values, this maps true/false into 1/0.</item>
	///		<item>For String values, this maps null and String.Empty into 0.</item>
	///		<item>For collection values, this maps empty collections into 0.</item>
	///		<item>For other values, this maps null into 0.</item>
	/// </list>
	/// 
	/// <para>If the ConverterParameter is set to Inverse, then 1 is returned in place of 0, and vice-versa.</para>
	/// 
	/// <para>You can use this to implement invisibility by opacity, in cases where real invisibility
	/// by collapsing the control would have undesirable side-effects on the control behavior.</para>
	/// </summary>
	public sealed class OpacityConverter : IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (targetType != typeof(double))
			{
				throw new ArgumentOutOfRangeException(nameof(targetType), "OpacityConverter can only convert to double");
			}

			double result = 1;

			if (value == null)
			{
				result = 0;
			}
			else if (value is bool)
			{
				result = (bool)value ? 1 : 0;
			}
			else if (value is string)
			{
				result = String.IsNullOrEmpty((string)value) ? 0 : 1;
			}
			else if (value is IEnumerable)
			{
				IEnumerable enumerable = (IEnumerable)value;
				if (enumerable.GetEnumerator().MoveNext() == false)
				{
					result = 0;
				}
			}

			if ((parameter is string) &&
			    (String.Compare((string)parameter, "Inverse", StringComparison.OrdinalIgnoreCase) == 0))
			{
				result = (result == 1) ? 0 : 1;
			}

			return result;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is double))
			{
				throw new ArgumentOutOfRangeException(nameof(value), "OpacityConverter can only convert from double");
			}

			if (targetType == typeof(bool))
			{
				return ((double)value == 1) ? true : false;
			}

			throw new ArgumentOutOfRangeException(nameof(targetType), "OpacityConverter can only convert to Boolean");
		}
	}
}