namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Applies and restores console font colors with the assistance of a using block.
	/// </summary>
	public sealed class ColoredConsole : IDisposable
	{
		public ColoredConsole(ConsoleColor foregroundColor) : this(foregroundColor, Console.BackgroundColor)
		{
		}

		public ColoredConsole(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
		{
			_oldBackgroundColor = Console.BackgroundColor;
			_oldForegroundColor = Console.ForegroundColor;

			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			Console.BackgroundColor = _oldBackgroundColor;
			Console.ForegroundColor = _oldForegroundColor;
		}

		private bool _disposed;

		private ConsoleColor _oldBackgroundColor;
		private ConsoleColor _oldForegroundColor;
	}
}