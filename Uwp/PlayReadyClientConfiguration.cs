namespace Axinom.Toolkit
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Enables you to provide various configuration parameters for helper methods that operate with the PlayReady client API.
	/// </summary>
	public sealed class PlayReadyClientConfiguration
	{
		/// <summary>
		/// The URL of the license server. This value will override the URL in the content header.
		/// You must provide a value here if the content header does not contain a license server URL.
		/// </summary>
		public Uri LicenseServerUrl { get; set; }

		/// <summary>
		/// Contents to place in the CustomData field in the license challenge.
		/// </summary>
		public string AcquireLicenseCustomData { get; set; }

		/// <summary>
		/// Contents to place in the CustomData field when joining the domain (if joining a domain is required by the server).
		/// </summary>
		public string JoinDomainCustomData { get; set; }

		/// <summary>
		/// HTTP headers to add to the license acquisition HTTP request.
		/// </summary>
		public IDictionary<string, string> AcquireLicenseHttpHeaders { get; set; } = new Dictionary<string, string>();
	}
}