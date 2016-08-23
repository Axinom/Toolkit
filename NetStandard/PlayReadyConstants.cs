namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class PlayReadyConstants
	{
		/// <summary>
		/// Protection System ID used in various manifests and metadata.
		/// </summary>
		public static readonly Guid SystemId = new Guid("9A04F079-9840-4286-AB92-E65BE0885F95");

		public const string RightsManagementHeaderNamespace = "http://schemas.microsoft.com/DRM/2007/03/PlayReadyHeader";
	}
}