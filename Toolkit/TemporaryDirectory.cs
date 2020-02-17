namespace Axinom.Toolkit
{
	using System;
	using System.IO;

	/// <summary>
	/// This class simplifies the creation of temporary folders into  the current user's temporary files folder
	/// or an equivalent folder of your choosing.
	/// 
	/// You can use the class in combination with the using statement to automatically delete the folder
	/// once you have finished using it.
	/// </summary>
	public sealed class TemporaryDirectory : IDisposable
	{
		public string Path { get; private set; }

		/// <summary>
		/// Creates a temporary directory with a random name.
		/// </summary>
		public TemporaryDirectory() : this(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString())
		{
		}

		/// <summary>
		/// Creates a temporary directory with a custom prefix on the directory name.
		/// </summary>
		public TemporaryDirectory(string prefix) : this(System.IO.Path.GetTempPath(), prefix + "-" + Guid.NewGuid())
		{
		}

		private TemporaryDirectory(string parent, string name)
		{
			Path = System.IO.Path.Combine(parent, name);
			Directory.CreateDirectory(Path);
		}

		/// <summary>
		/// Creates a temporary directory in a custom location, instead of the user's temporary files folder.
		/// </summary>
		public static TemporaryDirectory WithParentDirectory(string parentPath)
		{
			return new TemporaryDirectory(parentPath, Guid.NewGuid().ToString());
		}

		/// <summary>
		/// Creates a temporary directory in a custom location, instead of the user's temporary files folder.
		/// Assigns a custom prefix on the directory name.
		/// </summary>
		public static TemporaryDirectory WithParentDirectory(string parentPath, string prefix)
		{
			return new TemporaryDirectory(parentPath, prefix + "-" + Guid.NewGuid());
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