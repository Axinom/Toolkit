namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Implements a weak event listener that allows the owner to be garbage
	/// collected if its only remaining link is an event handler.
	/// </summary>
	/// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
	/// <typeparam name="TSource">Type of source for the event.</typeparam>
	/// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    /// <remarks>Disposal of the instance is thread-safe. Do not modify callbacks during use.</remarks>
	public sealed class WeakEventListener<TInstance, TSource, TEventArgs> : IDisposable where TInstance : class
	{
		/// <summary>
		/// WeakReference to the instance listening for the event.
		/// </summary>
		private readonly WeakReference _weakInstance;

        /// <summary>
        /// Gets or sets the method to call when the event fires.
        /// </summary>
        /// <remarks>
        /// Not safe to modify after any methods on the instance have been called.
        /// </remarks>
        public Action<TInstance, TSource, TEventArgs> OnEventAction { get; set; }

        /// <summary>
        /// Gets or sets the method to call when detaching from the event.
        /// Guaranteed to be called only once (or never, if reference stays alive forever).
        /// </summary>
        /// <remarks>
        /// Not safe to modify after any methods on the instance have been called.
        /// </remarks>
        public Action<WeakEventListener<TInstance, TSource, TEventArgs>> OnDetachAction { get; set; }

		/// <summary>
		/// Initializes a new instances of the WeakEventListener class.
		/// </summary>
		/// <param name="instance">Instance subscribing to the event.</param>
		public WeakEventListener(TInstance instance)
		{
			Helpers.Argument.ValidateIsNotNull(instance, nameof(instance));

			_weakInstance = new WeakReference(instance);
		}

		/// <summary>
		/// Handler for the subscribed event calls OnEventAction to handle it.
		/// </summary>
		/// <param name="source">Event source.</param>
		/// <param name="eventArgs">Event arguments.</param>
		public void OnEvent(TSource source, TEventArgs eventArgs)
		{
			TInstance target = (TInstance)_weakInstance.Target;

			if (null != target)
			{
				// Call registered action
				OnEventAction?.Invoke(target, source, eventArgs);
			}
			else
			{
				// Detach from event
				Detach();
			}
		}

		void IDisposable.Dispose()
		{
			Detach();
		}

        private readonly object _detachLock = new object();

		/// <summary>
		/// Detaches from the subscribed event.
		/// </summary>
		public void Detach()
		{
            lock (_detachLock)
            {
                if (null != OnDetachAction)
                {
                    OnDetachAction(this);
                    OnDetachAction = null;
                }
            }
		}
	}
}