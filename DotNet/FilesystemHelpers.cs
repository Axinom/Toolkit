namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Web.Hosting;

	public static partial class DotNetHelpers
	{
		/// <summary>
		/// On non-Microsoft operating systems, grants execute permissions to the file at the specified path.
		/// On Microsoft operating systems, does nothing.
		/// </summary>
		/// <remarks>
		/// Permissions are granted to everyone. Do it manually if you wish to be more restrictive.
		/// </remarks>
		public static void EnsureExecutePermission(this HelpersContainerClasses.Filesystem container, string path)
		{
			if (!Helpers.Environment.IsNonMicrosoftOperatingSystem())
				return;

			// End result of all that escaping: sh -c "chmod +x \"/some/nice file\""
			ExternalTool.Execute("sh", string.Format("-c \"chmod +x \\\"{0}\\\"\"", path), TimeSpan.FromSeconds(1));
		}

		/// <summary>
		/// Gets an absolute path for a web app, when given either an absolute path or a path relative to the application root.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="relativeOrAbsolutePath"/> is null.</exception>
		public static string GetAbsolutePathForWebApp(this HelpersContainerClasses.Filesystem container, string relativeOrAbsolutePath)
		{
			Helpers.Argument.ValidateIsNotNullOrWhitespace(relativeOrAbsolutePath, "relativeOrAbsolutePath");

			string virtualDirectoryPath = HostingEnvironment.ApplicationPhysicalPath;

			if (virtualDirectoryPath == null)
				throw new InvalidOperationException("This method can only be called in a web application.");

			if (Path.IsPathRooted(relativeOrAbsolutePath))
				return relativeOrAbsolutePath;
			else
				return Path.Combine(virtualDirectoryPath, relativeOrAbsolutePath);
		}
	}
}