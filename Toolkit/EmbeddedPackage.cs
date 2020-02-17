namespace Axinom.Toolkit
{
    using System;
    using System.IO;
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
        public EmbeddedPackage(Assembly assembly, string @namespace, params string[] filenames) : this(assembly, @namespace, null, filenames)
        {
        }

        /// <summary>
        /// Extracts an embedded package from the specified assembly and enables its contents to be accessed.
        /// </summary>
        /// <param name="assembly">The assembly from which the package is to be loaded.</param>
        /// <param name="namespace">The namespace of the files contained within the package (e.g. My.Stuff.Package1).</param>
        /// <param name="filenames">The names of the files to extract.</param>
        /// <param name="storageDirectory">Where to store the extracted files.</param>
        public EmbeddedPackage(Assembly assembly, string @namespace, TemporaryDirectory storageDirectory, params string[] filenames)
        {
            Helpers.Argument.ValidateIsNotNull(assembly, nameof(assembly));
            Helpers.Argument.ValidateIsNotNullOrWhitespace(@namespace, nameof(@namespace));
            Helpers.Argument.ValidateIsNotNull(filenames, nameof(filenames));

            if (filenames.Length == 0)
                throw new ArgumentException("No filenames specified.", nameof(filenames));

            if (storageDirectory == null)
                storageDirectory = new TemporaryDirectory();

            _directory = storageDirectory;

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

        private static readonly LogSource _log = Log.Default.CreateChildSource(nameof(EmbeddedPackage));
    }
}