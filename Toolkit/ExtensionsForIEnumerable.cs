namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class ExtensionsForIEnumerable
	{
		/// <summary>
		/// Creates an <see cref="ObservableCollection{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// The created collection is a copy and is not updated when the source is updated.
		/// </summary>
		public static ObservableCollection<TItem> ToObservableCollection<TItem>(this IEnumerable<TItem> instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			var result = new ObservableCollection<TItem>();

			foreach (var item in instance)
				result.Add(item);

			return result;
		}
	}
}