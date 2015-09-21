namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Data;
	using Windows.UI.Xaml.Markup;

	/// <summary>
	/// Converts values to other values via one-to-one mapping through a ResourceDictionary.
	/// The input values are put through .ToString() before mapping, to get the ResourceDictionary key.
	/// </summary>
	[ContentProperty(Name = "Mappings")]
	public sealed class MappingConverter : IValueConverter
	{
		public ResourceDictionary Mappings { get; set; } = new ResourceDictionary();

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			var s = value.ToString();

			if (Mappings == null || !Mappings.ContainsKey(s))
				return s;

			return Mappings[s] ?? s;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}