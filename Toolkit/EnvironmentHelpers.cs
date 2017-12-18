namespace Axinom.Toolkit
{
    using System;
    using System.Linq;

    public static partial class NetStandardHelpers
    {
        /// <summary>
        /// Gets whether the code is running on a Microsoft operating system that can be assumed to resemble Windows.
        /// </summary>
        public static bool IsMicrosoftOperatingSystem(this HelpersContainerClasses.Environment container)
        {
            // Just a little helper property to make code using these properties more clear.
            return !Helpers.Environment.IsNonMicrosoftOperatingSystem();
        }

        /// <summary>
        /// Gets whether the code is running on a non-Microsoft operating system that can be assumed to resemble Linux.
        /// </summary>
        public static bool IsNonMicrosoftOperatingSystem(this HelpersContainerClasses.Environment container)
        {
            return _isNonMicrosoftOperatingSystem.Value;
        }

        private static readonly PlatformID[] MicrosoftPlatforms =
        {
            PlatformID.Win32NT,
            PlatformID.Win32S,
            PlatformID.Win32Windows,
            PlatformID.WinCE,
            PlatformID.Xbox,
        };

        private static readonly Lazy<bool> _isNonMicrosoftOperatingSystem = new Lazy<bool>(() => !MicrosoftPlatforms.Contains(Environment.OSVersion.Platform));
    }
}