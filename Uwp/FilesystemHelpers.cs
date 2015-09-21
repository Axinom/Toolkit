namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Windows.Storage;

	public static partial class UwpHelpers
	{
		/// <summary>
		/// Returns the amount of free space (in bytes) in the provided storage folder.
		/// </summary>
		public static async Task<ulong> GetFreeSpaceAsync(this HelpersContainerClasses.Filesystem container, StorageFolder folder)
		{
			Helpers.Argument.ValidateIsNotNull(folder, nameof(folder));

			var properties = await folder.GetBasicPropertiesAsync().IgnoreContext();
			var filteredProperties = await properties.RetrievePropertiesAsync(new[] { "System.FreeSpace" }).IgnoreContext();
			return (ulong)filteredProperties["System.FreeSpace"];
		}
	}
}