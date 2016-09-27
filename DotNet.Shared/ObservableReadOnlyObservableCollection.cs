namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Linq;

	/// <summary>
	/// This class exists to work around the bizarre fact that the <b>CollectionChanged</b> event of the
	/// <see cref="ReadOnlyObservableCollection{T}"/> class is protected. You can still access it via explicit interface
	/// casting but that is too bothersome.
	/// </summary>
	public class ObservableReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>
	{
		public ObservableReadOnlyObservableCollection(ObservableCollection<T> wrappedCollection)
			: base(wrappedCollection)
		{
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			base.OnCollectionChanged(args);

			CollectionChanged(this, args);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			base.OnPropertyChanged(args);

			PropertyChanged(this, args);
		}

		public new event PropertyChangedEventHandler PropertyChanged = delegate { };
		public new event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
	}
}