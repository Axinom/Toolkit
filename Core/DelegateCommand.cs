namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Input;

	/// <summary>
	/// <see cref="ICommand"/> implementation that outsources all responsibility via delegates.
	/// </summary>
	/// <remarks>
	/// For internal consistency, a <see cref="CanExecute"/> check is always done before <see cref="Execute"/>.
	/// An exception is thrown when an attempt is made to execute in a state where execution is not possible.
	/// </remarks>
	public sealed class DelegateCommand : ICommand
	{
		/// <summary>
		/// Delegate for <see cref="ICommand.Execute"/>. If not provided, the command does nothing.
		/// </summary>
		public Action<object> Execute { get; set; }

		/// <summary>
		/// Delegate for <see cref="ICommand.CanExecute"/>. If not provided, the command can always be executed.
		/// </summary>
		public Func<object, bool> CanExecute { get; set; }

		bool ICommand.CanExecute(object parameter)
		{
			if (CanExecute == null)
				return true;

			return CanExecute(parameter);
		}

		void ICommand.Execute(object parameter)
		{
			if (!((ICommand)this).CanExecute(parameter))
				throw new InvalidOperationException("Cannot call Execute when CanExecute returns false.");

			if (Execute == null)
				return; // Well this was pointless :)

			Execute(parameter);
		}

		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Allows you to raise the <see cref="CanExecuteChanged"/> event from outside DelegateCommand.
		/// If you do not call this method, <see cref="CanExecuteChanged"/> is never raised.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			#region Raise CanExecuteChanged(this, EventArgs.Empty)
			{
				var eventHandler = CanExecuteChanged;
				if (eventHandler != null)
					eventHandler(this, EventArgs.Empty);
			}
			#endregion
		}

		public DelegateCommand()
		{
		}

		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			Execute = execute;
			CanExecute = canExecute;
		}
	}
}