namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Diagnostics;
	using System.Linq;
	using System.Xml.Serialization;

	/// <summary>
	/// Configuration for the <see cref="ErrorReport"/> class. Loaded from the "ErrorReports" configuration section.
	/// </summary>
	/// <remarks>
	/// If this element is missing, no error notifications are sent by <see cref="ErrorReport"/>.
	/// </remarks>
	/// <seealso cref="DynamicConfigurationSection"/>
	[XmlRoot(ElementName = "ErrorReports")]
	public sealed class ErrorReportConfiguration
	{
		public static ErrorReportConfiguration Current
		{
			get
			{
				try
				{
					return (ErrorReportConfiguration)ConfigurationManager.GetSection("ErrorReports");
				}
				catch (Exception ex)
				{
					Trace.TraceError("ErrorReports configuration section could not be loaded: " + ex.Message);

					return null;
				}
			}
		}

		/// <summary>
		/// Gets whether error reports have been configured and sending e-mails is enabled.
		/// </summary>
		internal static bool IsConfiguredAndShouldSendMail
		{
			get { return Current != null && Current.SendMail; }
		}

		/// <summary>
		/// Gets whether error reports have been configured and writing to the event log is enabled.
		/// </summary>
		internal static bool IsConfiguredAndShouldWriteToEventLog
		{
			get { return Current != null && Current.WriteToEventLog; }
		}

		/// <summary>
		/// Whether errors should be sent to a mailbox.
		/// </summary>
		public bool SendMail;

		/// <summary>
		/// Whether errors should be written to the event log.
		/// </summary>
		public bool WriteToEventLog;

		/// <summary>
		/// The e-mail address to send error reports to.
		/// </summary>
		public string ReceiverAddress;

		/// <summary>
		/// The name of the event source to log errors under.
		/// </summary>
		public string EventSource;

		/// <summary>
		/// The name of the event log to log errors under. If blank, Application is used.
		/// </summary>
		public string EventLog;
	}
}