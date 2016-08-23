namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Thrown if there is a nonspecific failure coming from the environment (i.e. anything external to code).
	/// The primary purpose is to allow easy differentiation between logic related failures and environment related failures.
	/// E.g. connection failure, file not found, invalid data loaded, etc.
	/// </summary>
	public class EnvironmentException : Exception
	{
		public EnvironmentException()
		{
		}

		public EnvironmentException(string message) : base(message)
		{
		}

		public EnvironmentException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}