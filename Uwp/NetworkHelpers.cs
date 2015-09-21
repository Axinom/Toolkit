namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Windows.Networking.Connectivity;

	public static partial class UwpHelpers
	{
		public static bool IsInternetAvailable(this HelpersContainerClasses.Network container)
		{
			var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

			return internetConnectionProfile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
		}
	}
}