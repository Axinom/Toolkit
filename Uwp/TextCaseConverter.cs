namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml.Data;

	public enum TextCase
	{
		Lowercase,
		Uppercase,
		FirstLetterUppercase
	}

	/// <summary>
	/// Converts a string to a specific <see cref="TextCase"/>.
	/// </summary>
	public sealed class TextCaseConverter : IValueConverter
	{
		/// <summary>
		/// Which case to convert text to.
		/// </summary>
		/// <value>Defaults to <see cref="TextCase.Lowercase"/>.</value>
		public TextCase Case { get; set; }

		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (targetType != typeof(string))
				throw new ArgumentException("TextCaseConverter can only convert to string.", nameof(targetType));

			if (value == null)
				return null;

			if (!(value is string))
				throw new ArgumentException("TextCaseConverter can only take string as input.", nameof(value));

			var s = (string)value;

			if (s == "")
				return "";

			if (Case == TextCase.Lowercase)
			{
				return s.ToLower();
			}
			else if (Case == TextCase.Uppercase)
			{
				return s.ToUpper();
			}
			else if (Case == TextCase.FirstLetterUppercase)
			{
				if (s.Length == 1)
					return s.ToUpper();

				return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
			}
			else
				throw new NotSupportedException("Unexpected TextCase: " + Case);
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}