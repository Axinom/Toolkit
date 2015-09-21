namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Windows.Media.Protection.PlayReady;

	public static partial class UwpHelpers
	{
		/// <summary>
		/// Ensures that PlayReady is activated for the current application. Any other PlayReady operations may fail
		/// before activation, so you are recommended to activate at the earliest possible opportunity.
		/// </summary>
		public static async Task EnsureActivatedAsync(this HelpersContainerClasses.PlayReady container)
		{
			try
			{
				// This will throw if PlayReady is not activated.
				var temp = PlayReadyStatics.PlayReadySecurityVersion;

				Log.Default.Info("PlayReady is already activated.");
				return;
			}
			catch (Exception ex)
			{
				// MSPR_E_NEEDS_INDIVIDUALIZATION
				if (ex.HResult != unchecked((int)0x8004B822))
					throw;

				Log.Default.Info("PlayReady requires activation.");

				var request = new PlayReadyIndividualizationServiceRequest();
				await request.BeginServiceRequest().IgnoreContext();

				Log.Default.Info("PlayReady activated.");
			}
		}

		/// <summary>
		/// Proactively acquires a persistent PlayReady license, provided the key ID and the license server URL.
		/// 
		/// Note that while this method will succeed even if a non-persistent license is returned, such a license
		/// cannot be later used for playback. You need a persistent license for proactive license acquisition.
		/// </summary>
		public static async Task AcquirePersistentLicenseAsync(this HelpersContainerClasses.PlayReady container, Guid keyId, Uri licenseServerUrl, string challengeCustomData = null)
		{
			Helpers.Argument.ValidateIsNotNull(licenseServerUrl, nameof(licenseServerUrl));
			Helpers.Argument.ValidateIsAbsoluteUrl(licenseServerUrl, nameof(licenseServerUrl));

			var header = new PlayReadyContentHeader(Helpers.PlayReady.GenerateRightsManagementHeader(keyId));
			var request = new PlayReadyLicenseAcquisitionServiceRequest();
			request.ContentHeader = header;
			request.Uri = licenseServerUrl;

			if (challengeCustomData != null)
				request.ChallengeCustomData = challengeCustomData;

			await request.BeginServiceRequest().IgnoreContext();
		}

		/// <summary>
		/// Checks whether a usable persistent license is present for the provided key ID.
		/// </summary>
		public static bool IsPersistentLicensePresent(this HelpersContainerClasses.PlayReady container, Guid keyId)
		{
			var header = new PlayReadyContentHeader(Helpers.PlayReady.GenerateRightsManagementHeader(keyId));
			return new PlayReadyLicenseIterable(header, true).Any(l => l.UsableForPlay);
		}
	}
}