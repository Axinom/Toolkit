namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// This class simplifies the creation of temporary folders into the current user's temporary files folder.
	/// You can use the class in combination with the using statement to automatically delete the folder
	/// once you have finished using it.
	/// </summary>
	public sealed class TemporaryDirectory : IDisposable
	{
		public string Path { get; private set; }

		/// <summary>
		/// Creates a temporary directory with a random name.
		/// </summary>
		public TemporaryDirectory()
		{
			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(Path);
		}

		/// <summary>
		/// Creates a temporary directory with a custom prefix on the directory name.
		/// </summary>
		public TemporaryDirectory(string prefix)
		{
			Helpers.Argument.ValidateIsNotNullOrWhitespace(prefix, nameof(prefix));

			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), prefix + "-" + Guid.NewGuid());
			Directory.CreateDirectory(Path);
		}

		public void Delete()
		{
			try
			{
				Directory.Delete(Path, true);
			}
			catch
			{
				// Whatever!
			}
		}

		void IDisposable.Dispose()
		{
			Delete();
		}
	}
}