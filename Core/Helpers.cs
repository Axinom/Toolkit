namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Entry point to Axinom Toolkit helper methods.
	/// </summary>
	/// <remarks>
	/// This class references various container classes that are provided extension methods by the different Axinom Toolkit
	/// libraries. This style exists because there is some overlap between the different platforms (e.g. we want to offer
	/// general purpose PlayReady helper methods and also specialized UWP PlayReady helper methods) and extension methods
	/// are the only sensible way to do this without forcing the caller to dig for the right platform-specific class.
	/// </remarks>
	public static class Helpers
	{
		public static readonly HelpersContainerClasses.Argument Argument = new HelpersContainerClasses.Argument();
		public static readonly HelpersContainerClasses.Asf Asf = new HelpersContainerClasses.Asf();
		public static readonly HelpersContainerClasses.Async Async = new HelpersContainerClasses.Async();
		public static readonly HelpersContainerClasses.Com Com = new HelpersContainerClasses.Com();
		public static readonly HelpersContainerClasses.Convert Convert = new HelpersContainerClasses.Convert();
		public static readonly HelpersContainerClasses.DataContract DataContract = new HelpersContainerClasses.DataContract();
		public static readonly HelpersContainerClasses.Debug Debug = new HelpersContainerClasses.Debug();
		public static readonly HelpersContainerClasses.Environment Environment = new HelpersContainerClasses.Environment();
		public static readonly HelpersContainerClasses.Filesystem Filesystem = new HelpersContainerClasses.Filesystem();
		public static readonly HelpersContainerClasses.Guid Guid = new HelpersContainerClasses.Guid();
		public static readonly HelpersContainerClasses.Media Media = new HelpersContainerClasses.Media();
		public static readonly HelpersContainerClasses.Network Network = new HelpersContainerClasses.Network();
		public static readonly HelpersContainerClasses.NLog NLog = new HelpersContainerClasses.NLog();
		public static readonly HelpersContainerClasses.PlayReady PlayReady = new HelpersContainerClasses.PlayReady();
		public static readonly HelpersContainerClasses.Random Random = new HelpersContainerClasses.Random();
		public static readonly HelpersContainerClasses.Struct Struct = new HelpersContainerClasses.Struct();
		public static readonly HelpersContainerClasses.Type Type = new HelpersContainerClasses.Type();
		public static readonly HelpersContainerClasses.WebPath WebPath = new HelpersContainerClasses.WebPath();
		public static readonly HelpersContainerClasses.WindowsMediaDrm WindowsMediaDrm = new HelpersContainerClasses.WindowsMediaDrm();
		public static readonly HelpersContainerClasses.XmlSerialization XmlSerialization = new HelpersContainerClasses.XmlSerialization();
	}
}