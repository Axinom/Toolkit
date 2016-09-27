namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Used in to ensure that a piece of code that should never be reachable will not accidentally execute.
	/// </summary>
	public sealed class UnreachableCodeException : Exception
	{
	}
}