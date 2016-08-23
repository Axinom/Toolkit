namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Basic container that does not hold any references to the items it contains.
	/// Exposes only basic functions to keep the implementation simple.
	/// </summary>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// class NavigationManager
	/// {
	///		private WeakContainer<Page> _cache = new WeakContainer<Page>();
	///		
	///		protected void OnNavigating(Uri pageUrl)
	///		{
	///			foreach (Page page in _cache)
	///			{
	///				if (page.Url == pageUrl)
	///				{
	///					// Found cached page!
	///					CurrentPage = page;
	///					return;
	///				}
	///			}
	///			
	///			// Page not in cache or was already garbage collected. Create new instance.
	///			Page page = InitializePage(pageUrl);
	///			_cache.Add(page);
	///			CurrentPage = page;
	///		}
	///		
	///		// [...]
	/// ]]>
	/// </code>
	/// </example>
	/// <threadsafety instance="false" />
	public sealed class WeakContainer<T> : IEnumerable<T>, IEnumerable
		where T : class
	{
		private readonly List<WeakReference> _items = new List<WeakReference>();

		/// <summary>
		/// Adds an item to the container by WeakReference.
		/// The same item can be added multiple times.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
		public void Add(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Cleanup();

			_items.Add(new WeakReference(item));
		}

		/// <summary>
		/// Removes an item from the container, if the container has it.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
		public void Remove(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Cleanup();

			foreach (var wr in _items)
			{
				T target = (T)wr.Target;

				if (target == item)
				{
					_items.Remove(wr);
					return;
				}
			}
		}

		/// <summary>
		/// Checks whether the container has an item.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
		public bool Contains(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Cleanup();

			foreach (var wr in _items)
			{
				T target = (T)wr.Target;

				if (target == item)
				{
					return true;
				}
			}

			return false;
		}

		private void Cleanup()
		{
			// OPTIMIZATION: Do not perform cleanup on every operation.

			List<WeakReference> dead = new List<WeakReference>();

			foreach (var wr in _items)
			{
				if (wr.Target == null)
				{
					dead.Add(wr);
				}
			}

			dead.ForEach(wr => _items.Remove(wr));
		}

		#region IEnumerable
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var wr in _items)
			{
				T target = (T)wr.Target;

				if (target == null)
					continue;

				yield return target;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var wr in _items)
			{
				T target = (T)wr.Target;

				if (target == null)
					continue;

				yield return target;
			}
		}
		#endregion
	}
}