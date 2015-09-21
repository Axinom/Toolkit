namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Input;

	/// <summary>
	/// Extensions for <see cref="ICommand"/>.
	/// </summary>
	public static class ExtensionsForICommand
	{
		/// <summary>
		/// Executes the command but only if <see cref="ICommand.CanExecute"/> returns true.
		/// </summary>
		/// <remarks>
		/// Allows you to avoid needlessly verbose and possibly error-prone code of the following form:
		/// <code><![CDATA[
		/// if (command.CanExecute(param))
		///		command.Execute(param);
		/// ]]></code>
		/// </remarks>
		public static void TryExecute(this ICommand instance, object parameter)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (!instance.CanExecute(parameter))
				return;

			instance.Execute(parameter);
		}
	}
}