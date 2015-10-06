namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Provides the response custom data for exceptions triggered by a PlayReady request to a remote server.
	/// </summary>
	public sealed class PlayReadyRequestException : Exception
	{
		public string ResponseCustomData { get; }

		public PlayReadyRequestException(Exception original, string responseCustomData) : base(original.Message, original.InnerException)
		{
			Helpers.Argument.ValidateIsNotNull(original, nameof(original));

			HResult = original.HResult;
			ResponseCustomData = responseCustomData;
		}
	}
}