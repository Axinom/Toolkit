namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Thrown if some piece of code violates the contract it is supposed to follow (e.g. returns null when it must not).
	/// </summary>
	public class ContractException : Exception
	{
		public ContractException()
		{
		}

		public ContractException(string message) : base(message)
		{
		}

		public ContractException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}