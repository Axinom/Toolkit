namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Extensions for <see cref="ICollection{T}"/>.
	/// </summary>
	public static class ExtensionsForICollection
	{
		/// <summary>
		/// Adds a range of items to a collection.
		/// </summary>
		/// <remarks>
		/// The items are added one-by-one, so all CollectionChanged events and such will be raised for each item separately.
		/// </remarks>
		public static void AddRange<TItem>(this ICollection<TItem> instance, IEnumerable<TItem> items)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (items == null)
				throw new ArgumentNullException("items");

			if (instance == items)
				throw new ArgumentException("You cannot use AddRange() to add a collection to itself.", "items");

			foreach (var item in items)
				instance.Add(item);
		}

		/// <summary>
		/// Replaces the contents of a collection by adding or removing single items.
		/// Useful if you want to avoid removing items that will be re-added later or just switching collections.
		/// This case often occurs with UI, when you don't want to animate out/in items that didn't change.
		/// </summary>
		public static void UpdateCollection<TItem>(this ICollection<TItem> instance, IEnumerable<TItem> newContents)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (newContents == null)
				throw new ArgumentNullException("newContents");

			var removed = instance.Except(newContents).ToArray();
			var added = newContents.Except(instance).ToArray();

			foreach (var item in removed)
				instance.Remove(item);

			foreach (var item in added)
				instance.Add(item);
		}
	}
}