namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Windows.Foundation.Collections;
	using Windows.Media.Protection;
	using Windows.Media.Protection.PlayReady;

	/// <summary>
	/// Implements simple PlayReady license acquisition behavior and enables it to be associated with a MediaElement.
	/// </summary>
	public sealed class PlayReadyLicenseAcquirer
	{
		/// <summary>
		/// Gets or sets the PlayReady client configuration to use for performing any license acquisition requests.
		/// </summary>
		public PlayReadyClientConfiguration Configuration { get; set; }

		/// <summary>
		/// Gets the Media Protection Manager instance associated with this license acquirer.
		/// Assign this Media Protection Manager to a MediaElement in order to use the license acquirer.
		/// </summary>
		public MediaProtectionManager MediaProtectionManager { get; } = new MediaProtectionManager();

		/// <summary>
		/// Raised when a PlayReady operation fails. If the exception is PlayReadyRequestException, it will contain
		/// the error code and any response custom data (if the failure is signaled by a response from the license server).
		/// 
		/// The event is raised using the same scheduler that was active when the constructor was called.
		/// </summary>
		public event EventHandler<ErrorEventArgs> Failed;

		public PlayReadyLicenseAcquirer()
		{
			MediaProtectionManager.ServiceRequested += ProtectionManagerOnServiceRequested;
			MediaProtectionManager.ComponentLoadFailed += ProtectionManagerOnComponentLoadFailed;
			MediaProtectionManager.RebootNeeded += ProtectionManagerOnRebootNeeded;

			// Some magic configuration to turn on PlayReady.
			MediaProtectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemIdMapping", new PropertySet
			{
				{ "{F4637010-03C3-42CD-B932-B48ADF3A6A54}", "Windows.Media.Protection.PlayReady.PlayReadyWinRTTrustedInput" }
			});
			MediaProtectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionSystemId", "{F4637010-03C3-42CD-B932-B48ADF3A6A54}");
			MediaProtectionManager.Properties.Add("Windows.Media.Protection.MediaProtectionContainerGuid", "{9A04F079-9840-4286-AB92-E65BE0885F95}");

			// We raise events on whatever task scheduler is active when the instance is created.
			_taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		}

		private void ProtectionManagerOnRebootNeeded(MediaProtectionManager sender)
		{
			_log.Warning("Reboot is required in order to continue with PlayReady playback.");
		}

		private async void ProtectionManagerOnServiceRequested(MediaProtectionManager sender, ServiceRequestedEventArgs e)
		{
			if (!(e.Request is IPlayReadyServiceRequest))
			{
				_log.Error($"Unknown type of media protection service request: {e.Request.GetType().Name} ({e.Request.Type} for system {e.Request.ProtectionSystem}).");
				e.Completion.Complete(false);
				return;
			}

			try
			{
				await Helpers.PlayReady.FulfillServiceRequestAsync((IPlayReadyServiceRequest)e.Request, Configuration);
				e.Completion.Complete(true);
			}
			catch (Exception ex)
			{
				if (ex is PlayReadyRequestException)
					_log.Error($"Failure when handling PlayReady service request: {ex.Message} (0x{ex.HResult:X}) with ResponseCustomData: {((PlayReadyRequestException)ex).ResponseCustomData}");
				else
					_log.Error($"Failure when handling PlayReady service request: {ex.Message} (0x{ex.HResult:X})");

				e.Completion.Complete(false);
				Task.Factory.StartNew(() => Failed?.Invoke(this, new ErrorEventArgs(ex)), CancellationToken.None, TaskCreationOptions.None, _taskScheduler).Forget();
			}
		}

		private void ProtectionManagerOnComponentLoadFailed(MediaProtectionManager sender, ComponentLoadFailedEventArgs e)
		{
			_log.Error("PlayReady component load failed: " + Helpers.Debug.ToDebugString(e));

			e.Completion.Complete(false);
		}

		private readonly TaskScheduler _taskScheduler;

		private static readonly LogSource _log = Log.Default.CreateChildSource(nameof(PlayReadyLicenseAcquirer));
	}
}