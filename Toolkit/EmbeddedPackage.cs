namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// Extracts a temporary copy of an embedded package consisting of one or more files
	/// provides easy access to the package contents while the instance remains alive.
	/// </summary>
	public sealed class EmbeddedPackage : IDisposable
	{
		/// <summary>
		/// Gets the path to the directory that contains the package.
		/// </summary>
		public string Path => _directory?.Path;

		/// <summary>
		/// Extracts an embedded package from the specified assembly and enables its contents to be accessed.
		/// </summary>
		/// <param name="assembly">The assembly from which the package is to be loaded.</param>
		/// <param name="namespace">The namespace of the files contained within the package (e.g. My.Stuff.Package1).</param>
		/// <param name="filenames">The names of the files to extract.</param>
		public EmbeddedPackage(Assembly assembly, string @namespace, params string[] filenames)
		{
			Helpers.Argument.ValidateIsNotNull(assembly, nameof(assembly));
			Helpers.Argument.ValidateIsNotNullOrWhitespace(@namespace, nameof(@namespace));
			Helpers.Argument.ValidateIsNotNull(filenames, nameof(filenames));

			if (filenames.Length == 0)
				throw new ArgumentException("No filenames specified.", nameof(filenames));

			_directory = new TemporaryDirectory();

			try
			{
				foreach (var filename in filenames)
				{
					var streamName = @namespace + "." + filename;
					var destinationPath = System.IO.Path.Combine(Path, filename);

					_log.Debug($"Extracting {streamName} to {destinationPath}.");

					using (var assemblyStream = assembly.GetManifestResourceStream(streamName))
					{
						if (assemblyStream == null)
							throw new ArgumentException($"Embedded file {streamName} not found.");

						using (var fileStream = File.Create(destinationPath))
							assemblyStream.CopyTo(fileStream);

						_keepaliveHandles.Add(File.Open(destinationPath, FileMode.Open, FileAccess.Read, FileShare.Read));
					}
				}
			}
			catch
			{
				_directory.Delete();
				throw;
			}
		}

		public void Dispose()
		{
			if (_directory != null)
			{
				foreach (var handle in _keepaliveHandles)
					handle.Close();

				_keepaliveHandles.Clear();

				_directory.Delete();
				_directory = null;

				GC.SuppressFinalize(this);
			}
		}

		~EmbeddedPackage()
		{
			Dispose();
		}

		private TemporaryDirectory _directory;

		// We keep an open handle to each extracted file to ensure the OS does not delete them.
		// This is important because sometimes "temp files cleanup" features will delete our files.
		private List<FileStream> _keepaliveHandles = new List<FileStream>();

		private static readonly LogSource _log = Log.Default.CreateChildSource(nameof(EmbeddedPackage));
	}
}