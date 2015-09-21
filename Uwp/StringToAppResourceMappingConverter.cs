namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.UI.Xaml;
	using Windows.UI.Xaml.Data;
	using Windows.UI.Xaml.Markup;

	/// <summary>
	/// Converts string values to values in application resources via one-to-one mapping.
	/// Any input value is put through .ToString() before mapping to get the local resource key.
	/// The local resource values are the keys for the resource in the application resources.
	/// </summary>
	/// <remarks>
	/// [Input] is key for [Local Resource (Mappings)] is key for [App Resource].
	/// </remarks>
	[ContentProperty(Name = "Mappings")]
	public sealed class StringToResourceMappingConverter : IValueConverter
	{
		/// <summary>
		/// The value should be the key of the item in the application resources dictionary.
		/// </summary>
		public ResourceDictionary Mappings { get; set; } = new ResourceDictionary();

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			var s = value.ToString();

			if (Mappings == null || !Mappings.ContainsKey(s))
				return null;

			var resourceKey = Mappings[s];

			object resourceValue;
			if (Application.Current.Resources.TryGetValue(resourceKey, out resourceValue))
				return resourceValue;

			_log.Warning($"Unable to find app resource: {resourceKey}");

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		private static readonly LogSource _log = Log.Default.CreateChildSource(nameof(StringToResourceMappingConverter));
	}
}