namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.Storage.Streams;

	public static class ExtensionsForIRandomAccessStream
	{
		/// <summary>
		/// Clears the stream of any data and seeks back to the beginning.
		/// </summary>
		public static void Clear(this IRandomAccessStream stream)
		{
			stream.Size = 0;
			stream.Seek(0);
		}
	}
}