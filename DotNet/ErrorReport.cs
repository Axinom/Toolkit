namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Mail;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Sends an error report to all configured error report receivers (e-mail and/or event log).
	/// </summary>
	/// <remarks>
	/// Receiver configuration is defined by <see cref="ErrorReportConfiguration"/>.
	/// 
	/// <para>The error report will automatically include various additional data such as HTTP and WCF context parameters
	/// and (if available and in appropriate context) the HTTP request body. Additional data can be specified manually.</para>
	/// 
	/// <para>Configuration of SMTP server is taken from standard System.Net e-mail configuration section, if available.
	/// As a fallback, the "SmtpServer" AppSetting is used (this is what AxCMS uses).</para>
	/// </remarks>
	/// <example><code><![CDATA[
	/// try
	/// {
	///		ExecuteOperation(stuff, things);
	///	}
	///	catch (Exception ex)
	///	{
	///		ErrorReport.Send(ex);
	///	}
	///	
	/// try
	/// {
	///		ExecuteOperation2(stuff, things);
	/// }
	/// catch (Exception ex)
	/// {
	///		// Here we provide some extra data manually.
	///		ErrorReport.Send(ex, new
	///		{
	///			IsManagementSystem = CMSConfigurationSettings.IsAxCMS,
	///			InputData = stuff
	///		}
	/// }
	/// ]]></code></example>
	public static class ErrorReport
	{
		private const string DefaultEventLog = "Application";

		private static readonly WeakContainer<Exception> _reportedExceptions = new WeakContainer<Exception>();

		public static void Send(Exception error)
		{
			// This overload remembers the exceptions that it reports and does not report the same exception twice.
			// Other overloads do not do this check because they also include extra data which may prove useful.
			if (_reportedExceptions.Contains(error))
				return;

			_reportedExceptions.Add(error);

			Send(error, null);
		}

		public static void Send(Exception error, NameValueCollection additionalData)
		{
			Send(ExceptionToString(error), additionalData);
		}

		public static void Send(Exception error, object additionalData)
		{
			Send(ExceptionToString(error), additionalData);
		}

		public static void Send(string error)
		{
			Send(error, null);
		}

		public static void Send(string error, object additionalData)
		{
			Send(error, TransformAdditionalData(additionalData));
		}

		private static NameValueCollection TransformAdditionalData(object data)
		{
			var result = new NameValueCollection();

			if (data == null)
				return result;

			result["AdditionalData"] = Helpers.Debug.ToDebugString(data);

			return result;
		}

		public static void Send(string error, NameValueCollection additionalData)
		{
			if (error == null)
				throw new ArgumentNullException("error");

			try
			{
				string reportBody = CreateMessageBody(error, additionalData);

				Trace.TraceError(reportBody);

				if (ErrorReportConfiguration.IsConfiguredAndShouldWriteToEventLog)
					WriteToEventLog(reportBody);

				if (ErrorReportConfiguration.IsConfiguredAndShouldSendMail)
					SendMail(error, reportBody);
			}
			catch (Exception ex)
			{
				// We can't do much else here, so...
				Trace.TraceError(ex.ToString());
			}
		}

		private const int MaxEventLogEntryBodySize = 15000;

		private static void WriteToEventLog(string notificationBody)
		{
			try
			{
				if (notificationBody.Length > MaxEventLogEntryBodySize)
					notificationBody = notificationBody.Remove(MaxEventLogEntryBodySize) + "[... entry truncated due to size]";

				MyEventLog.WriteEntry(notificationBody, EventLogEntryType.Error);
			}
			catch (Exception ex)
			{
				// We can't do much else here, so...
				Trace.TraceError(ex.ToString());
			}
		}

		private static EventLog MyEventLog
		{
			get
			{
				lock (EventLogLock)
				{
					if (_myEventLog == null)
					{
						string logName = DefaultEventLog;
						if (!string.IsNullOrEmpty(ErrorReportConfiguration.Current.EventLog))
							logName = ErrorReportConfiguration.Current.EventLog;

						_myEventLog = new EventLog(logName);
						_myEventLog.Source = ErrorReportConfiguration.Current.EventSource;
					}

					return _myEventLog;
				}
			}
		}

		private static readonly object EventLogLock = new object();
		private static EventLog _myEventLog;

		private static void SendMail(string error, string notificationBody)
		{
			try
			{
				using (var message = new MailMessage())
				{
					message.To.Add(ErrorReportConfiguration.Current.ReceiverAddress);

					message.Body = notificationBody;
					message.Subject = GenerateMessageTitle(error);
					message.From = new MailAddress("noreply@axinom.com"); // Meaningless, so we hardcode it.

					// NOTE: we use configuration file settings for network configuration, by default.
					var client = new SmtpClient();
					if (string.IsNullOrEmpty(client.Host))
					{
						// As a fallback, we'll try the "SmtpServer" AppSetting from host/from.
						client.Host = ConfigurationManager.AppSettings["SmtpServer"];
					}

					client.Send(message);
				}
			}
			catch (Exception ex)
			{
				// We can't do much else here, so...
				Trace.TraceError(ex.ToString());
			}
		}

		private static string GenerateMessageTitle(string error)
		{
			// Subject can only take 1 line. Some exceptions have more.
			string firstLineOfExceptionMessage;

			using (StringReader reader = new StringReader(error))
				firstLineOfExceptionMessage = reader.ReadLine();

			// The subject is the first line of the message, prepended by:
			// machine name, process name, remote WCF address OR remote HTTP address.
			string remoteEndpoint = null;
			if (HttpContext.Current != null)
				remoteEndpoint = HttpContext.Current.Request.UserHostAddress;

			if (remoteEndpoint == null)
				return string.Format("{0} : {1} : {2}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, firstLineOfExceptionMessage);
			else
				return string.Format("{0} : {1} : {2} : {3}", Environment.MachineName, Process.GetCurrentProcess().ProcessName, remoteEndpoint, firstLineOfExceptionMessage);
		}

		private const string SEPARATOR = "-------------------------------------------";

		private static string CreateMessageBody(string error, NameValueCollection additionalData)
		{
			StringBuilder body = new StringBuilder();

			body.Append(error);
			body.AppendLine();
			body.AppendLine();
			body.AppendFormat("Timestamp: {0}", DateTime.UtcNow.ToString());
			body.AppendLine();
			body.AppendLine();

			Action<string, Action> failsafeAppend = delegate(string name, Action a)
			{
				try
				{
					body.AppendLine();
					body.AppendLine(SEPARATOR);
					body.AppendLine();
					body.AppendLine(name);
					body.AppendLine();

					a();
				}
				catch (Exception ex)
				{
					body.AppendLine("Unable to gather data for this section: " + ex.Message);
				}
			};

			failsafeAppend("Additional data", delegate
			{
				if (additionalData == null || additionalData.Count == 0)
				{
					body.AppendLine("No additional data.");
					return;
				}

				foreach (string key in additionalData.AllKeys)
				{
					body.AppendFormat("{0}: {1}", key, additionalData[key]);
					body.AppendLine();
				}
			});

			failsafeAppend("Environment information", delegate
			{
				string[] properties = new[]
				{
					"CommandLine",
					"CurrentDirectory",
					"ExitCode",
					"HasShutdownStarted",
					"Is64BitOperatingSystem",
					"Is64BitProcess",
					"MachineName",
					"OSVersion",
					"ProcessorCount",
					"SystemDirectory",
					"SystemPageSize",
					"TickCount",
					"UserDomainName",
					"UserInteractive",
					"UserName",
					"Version",
					"WorkingSet"
				};

				WriteStaticProperties(body, typeof(Environment), null, properties);
			});

			failsafeAppend("ASP.NET server variables", delegate
			{
				var context = HttpContext.Current;

				if (context == null)
				{
					body.AppendLine("No ASP.NET context.");
					return;
				}

				foreach (string key in context.Request.ServerVariables.AllKeys)
				{
					try
					{
						body.AppendFormat("{0}: {1}", key, context.Request.ServerVariables[key]);
						body.AppendLine();
					}
					catch (Exception ex)
					{
						body.AppendFormat("Error reading server variable {0}: {1}", key, ex.Message);
						body.AppendLine();
					}
				}
			});

			failsafeAppend("ASP.NET HTTP request properties", delegate
			{
				var context = HttpContext.Current;

				if (context == null)
				{
					body.AppendLine("No ASP.NET context.");
					return;
				}

				if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
				{
					body.AppendFormat("Logged-in user: {0}", context.User.Identity.Name);
					body.AppendLine();
				}

				string[] properties = new[]
				{
					"ApplicationPath",
					"AppRelativeCurrentExecutionFilePath",
					"ContentEncoding",
					"ContentLength",
					"ContentType",
					"CurrentExecutionFilePath",
					"FilePath",
					"HttpMethod",
					"IsAuthenticated",
					"Path",
					"PathInfo",
					"PhysicalApplicationPath",
					"PhysicalPath",
					"RawUrl",
					"TotalBytes",
					"UrlReferrer",
					"UserAgent",
					"UserHostAddress",
					"UserHostName",
					"UserLanguages"
				};

				string[] complexProperties = new[]
				{
					"Form",
					"Headers",
					"QueryString"
				};

				WriteInstanceProperties(body, context.Request, properties, complexProperties);
			});

			return body.ToString();
		}

		private static void WriteInstanceProperties(StringBuilder body, object instance, IEnumerable<string> basicProperties, IEnumerable<string> complexProperties)
		{
			var instanceType = instance.GetType();

			if (basicProperties != null)
			{
				foreach (var propertyName in basicProperties)
				{
					try
					{
						var value = instanceType.GetProperty(propertyName).GetValue(instance, null);
						body.AppendFormat("{0}: {1}", propertyName, value);
						body.AppendLine();
					}
					catch (Exception ex)
					{
						body.AppendFormat("{0} could not be read: {1}", propertyName, ex.Message);
						body.AppendLine();
					}
				}

				body.AppendLine();
			}

			if (complexProperties != null)
			{
				foreach (var propertyName in complexProperties)
				{
					try
					{
						var value = instanceType.GetProperty(propertyName).GetValue(instance, null);
						body.AppendFormat("{0}: {1}", propertyName, Helpers.Debug.ToDebugString(value));
					}
					catch (Exception ex)
					{
						body.AppendFormat("{0} could not be read: {1}", propertyName, ex.Message);
						body.AppendLine();
					}
				}
			}
		}

		private static void WriteStaticProperties(StringBuilder body, Type type, IEnumerable<string> basicProperties, IEnumerable<string> complexProperties)
		{
			if (basicProperties != null)
			{
				foreach (var propertyName in basicProperties)
				{
					try
					{
						var value = type.GetProperty(propertyName).GetValue(null, null);
						body.AppendFormat("{0}: {1}", propertyName, value);
						body.AppendLine();
					}
					catch (Exception ex)
					{
						body.AppendFormat("{0} could not be read: {1}", propertyName, ex.Message);
						body.AppendLine();
					}
				}

				body.AppendLine();
			}

			if (complexProperties != null)
			{
				foreach (var propertyName in complexProperties)
				{
					try
					{
						var value = type.GetProperty(propertyName).GetValue(null, null);
						body.AppendFormat("{0}: {1}", propertyName, Helpers.Debug.ToDebugString(value));
					}
					catch (Exception ex)
					{
						body.AppendFormat("{0} could not be read: {1}", propertyName, ex.Message);
						body.AppendLine();
					}
				}
			}
		}

		private static string ExceptionToString(Exception ex)
		{
			string result = ex.ToString();

			if (ex is WebException)
			{
				try
				{
					result += Environment.NewLine + "Response body: " + new StreamReader(((WebException)ex).Response.GetResponseStream()).ReadToEnd();
				}
				catch
				{
				}
			}

			return result;
		}
	}
}