namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Windows.Media.Protection.PlayReady;

	public static partial class UwpHelpers
	{
		private static readonly LogSource PlayReadyLog = Log.Default.CreateChildSource("Helpers.PlayReady");

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

				PlayReadyLog.Info("PlayReady is already activated.");
				return;
			}
			catch (Exception ex)
			{
				// MSPR_E_NEEDS_INDIVIDUALIZATION
				if (ex.HResult != unchecked((int)0x8004B822))
					throw;

				PlayReadyLog.Info("PlayReady requires activation. Activating...");

				var request = new PlayReadyIndividualizationServiceRequest();
				await request.BeginServiceRequest().IgnoreContext();

				PlayReadyLog.Info("PlayReady has been activated.");
			}
		}


		/// <summary>
		/// Proactively acquires a persistent PlayReady license, provided the key ID and the license server URL. If the license
		/// server requires a domain to be joined, this is attempted automatically after which the license request is retried.
		/// 
		/// Note that while this method will succeed even if a non-persistent license is returned, such a license
		/// cannot be later used for playback. You need a persistent license for proactive license acquisition.
		/// </summary>
		/// <exception cref="PlayReadyRequestException">Thrown if the license server refuses a PlayReady request.</exception>
		public static async Task AcquirePersistentLicenseAsync(this HelpersContainerClasses.PlayReady container, Guid keyId, Uri licenseServerUrl, string acquireLicenseCustomData = null, string joinDomainCustomData = null)
		{
			Helpers.Argument.ValidateIsNotNull(licenseServerUrl, nameof(licenseServerUrl));
			Helpers.Argument.ValidateIsAbsoluteUrl(licenseServerUrl, nameof(licenseServerUrl));

			PlayReadyLog.Debug($"Acquiring persistent license for {keyId} from {licenseServerUrl}.");

			var header = new PlayReadyContentHeader(Helpers.PlayReady.GenerateRightsManagementHeader(keyId));
			var licenseRequest = new PlayReadyLicenseAcquisitionServiceRequest();
			licenseRequest.ContentHeader = header;
			licenseRequest.Uri = licenseServerUrl;

			await Helpers.PlayReady.FulfillServiceRequestAsync(licenseRequest, acquireLicenseCustomData, joinDomainCustomData).IgnoreContext();
		}

		/// <summary>
		/// Checks whether a usable persistent license is present for the provided key ID.
		/// </summary>
		public static bool IsPersistentLicensePresent(this HelpersContainerClasses.PlayReady container, Guid keyId)
		{
			var header = new PlayReadyContentHeader(Helpers.PlayReady.GenerateRightsManagementHeader(keyId));
			return new PlayReadyLicenseIterable(header, true).Any(l => l.UsableForPlay);
		}

		private const int MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED = unchecked((int)0x8004B895);

		/// <summary>
		/// Executes the standard logic requires to fulfill a PlayReady service request. You do not need to use this function
		/// unless you are dealing with very low-level PlayReady logic - everyday app needs are cared for by other methods.
		/// </summary>
		/// <exception cref="PlayReadyRequestException">Thrown if the license server refuses a PlayReady request.</exception>
		public static async Task FulfillServiceRequestAsync(this HelpersContainerClasses.PlayReady container, IPlayReadyServiceRequest serviceRequest, string acquireLicenseCustomData = null, string joinDomainCustomData = null)
		{
			Helpers.Argument.ValidateIsNotNull(serviceRequest, nameof(serviceRequest));

			PlayReadyLog.Debug($"Fulfilling PlayReady service request of type {serviceRequest.GetType().Name}.");

			while (serviceRequest != null)
			{
				try
				{
					if (serviceRequest is PlayReadyLicenseAcquisitionServiceRequest)
					{
						var request = (PlayReadyLicenseAcquisitionServiceRequest)serviceRequest;

						if (acquireLicenseCustomData != null)
							request.ChallengeCustomData = acquireLicenseCustomData;

						if (request.Uri == null)
						{
							throw new EnvironmentException("No license server URL was associated with the PlayReady license request. This generally indicates that the URL is not present in the content header and must be specified manually.");
						}
					}
					else if (serviceRequest is PlayReadyDomainJoinServiceRequest)
					{
						var request = (PlayReadyDomainJoinServiceRequest)serviceRequest;

						if (joinDomainCustomData != null)
							request.ChallengeCustomData = joinDomainCustomData;
					}

					PlayReadyLog.Debug(Helpers.Debug.ToDebugString(serviceRequest));

					await serviceRequest.BeginServiceRequest().IgnoreContext();

					PlayReadyLog.Debug("Successfully fulfilled PlayReady service request.");

					break; // If we got to this point then we are done!
				}
				catch (Exception ex) when (ex.HResult == MSPR_E_CONTENT_ENABLING_ACTION_REQUIRED)
				{
					serviceRequest = serviceRequest.NextServiceRequest();

					PlayReadyLog.Debug($"Fulfilling chained PlayReady service request of type {serviceRequest.GetType().Name}.");
				}
				catch (EnvironmentException)
				{
					throw;
				}
				catch (Exception ex)
				{
					PlayReadyLog.Error($"Failed to fulfill PlayReady service request: 0x{ex.HResult:X8}");

					// If it is a request for which we have response custom data, include it in a new exception.
					if (serviceRequest is PlayReadyLicenseAcquisitionServiceRequest)
					{
						throw new PlayReadyRequestException(ex, ((PlayReadyLicenseAcquisitionServiceRequest)serviceRequest).ResponseCustomData);
					}
					else if (serviceRequest is PlayReadyDomainJoinServiceRequest)
					{
						throw new PlayReadyRequestException(ex, ((PlayReadyDomainJoinServiceRequest)serviceRequest).ResponseCustomData);
					}

					throw;
				}
			}
		}
	}
}