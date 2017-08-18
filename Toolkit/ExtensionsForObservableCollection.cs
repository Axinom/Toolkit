namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Linq;

	public static class ExtensionsForObservableCollection
	{
		/// <summary>
		/// Weakly observes the observable collection during the lifetime of the anchor object or until disposed.
		/// </summary>
		public static IDisposable WeakObserve<T>(this ObservableCollection<T> collection, object lifetimeAnchor, Action<T> itemAdded, Action<T> itemRemoved)
		{
			Helpers.Argument.ValidateIsNotNull(collection, nameof(collection));
			Helpers.Argument.ValidateIsNotNull(lifetimeAnchor, nameof(lifetimeAnchor));
			Helpers.Argument.ValidateIsNotNull(itemAdded, nameof(itemAdded));
			Helpers.Argument.ValidateIsNotNull(itemRemoved, nameof(itemRemoved));

			// Not super memory efficient to duplicate the list but whatever. It works.
			var knownItems = new List<T>();

			var listener = new WeakEventListener<object, object, NotifyCollectionChangedEventArgs>(lifetimeAnchor);
			listener.OnDetachAction = (wel) => collection.CollectionChanged -= wel.OnEvent;
			collection.CollectionChanged += listener.OnEvent;
			listener.OnEventAction = (instance, source, args) =>
			{
				if (args.Action == NotifyCollectionChangedAction.Reset)
				{
					foreach (var item in knownItems)
						itemRemoved(item);

					knownItems.Clear();

					foreach (var item in collection)
					{
						knownItems.Add(item);
						itemAdded(item);
					}
				}
				else
				{
					if (args.OldItems != null)
					{
						foreach (T item in args.OldItems)
						{
							knownItems.Remove(item);
							itemRemoved(item);
						}
					}

					if (args.NewItems != null)
					{
						foreach (T item in args.NewItems)
						{
							knownItems.Add(item);
							itemAdded(item);
						}
					}
				}
			};

			// There may already be items in there, so do the needful.
			foreach (T item in collection)
			{
				knownItems.Add(item);
				itemAdded(item);
			}

			return listener;
		}
	}
}