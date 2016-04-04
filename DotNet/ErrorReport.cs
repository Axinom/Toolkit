namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Sends an error report to the logging system.
	/// </summary>
	public static class ErrorReport
	{
		/// <summary>
		/// Logs an error report with the logging system.
		/// </summary>
		/// <param name="exception">An exception that describes error that occurred.</param>
		/// <param name="additionalData">Any additional data you wish to attach to the report.</param>
		/// <param name="log">The log to write the output to. A suitable default will be selected if null.</param>
		public static void Log(Exception exception, object additionalData = null, LogSource log = null)
		{
			Helpers.Argument.ValidateIsNotNull(exception, nameof(exception));

			log = log ?? Toolkit.Log.Default.CreateChildSource(nameof(ErrorReport));
			try
			{
				var details = new StringBuilder();

				// First a "subject line" suitable for summarizing and email headering purposes.
				// We take the last (deepest) exception from the stack, as it is the root cause of the exception.
				details.AppendLine(Summarize(exception));

				details.AppendLine();
				details.AppendFormat("Timestamp: {0}", DateTimeOffset.UtcNow.ToString("u"));
				details.AppendLine();
				details.AppendFormat("Machine name: {0}", Environment.MachineName);
				details.AppendLine();
				details.AppendFormat("Operating system: {0}", Environment.OSVersion);
				details.AppendLine();
				details.AppendFormat("Current user: {0}\\{1}", Environment.UserDomainName, Environment.UserName);
				details.AppendLine();
				details.AppendFormat("CLR version: {0}", Environment.Version);
				details.AppendLine();

				if (HttpContext.Current != null)
				{
					// We cannot access Request in Application_Start because it will throw!
					// However, we can check if Handler is null and assume we cannot touch Request if so.
					if (HttpContext.Current.Handler != null)
					{
						details.AppendLine();
						details.AppendFormat("Connected peer: {0}", HttpContext.Current.Request.UserHostAddress);
						details.AppendLine();

						details.AppendFormat("Request URL: {0} {1}", HttpContext.Current.Request.HttpMethod, HttpContext.Current.Request.Url);
						details.AppendLine();

						details.AppendFormat("User agent: {0}", HttpContext.Current.Request.UserAgent);
						details.AppendLine();

						details.AppendLine("HTTP headers:");
						foreach (var header in HttpContext.Current.Request.Headers.AllKeys)
						{
							try
							{
								details.AppendFormat("    {0}: {1}", header, HttpContext.Current.Request.Headers[header]);
								details.AppendLine();
							}
							catch
							{
								// There have been known issues with HTTP header parsing in the past.
								// While the situations may be rare, we better be safe than sorry.
							}
						}

						details.AppendLine();
					}
				}

				details.AppendLine();
				details.AppendLine("Loaded non-system assemblies:");
				var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName());

				// We filter out system assemblies because they are boring.
				foreach (var assemblyName in loadedAssemblyNames
					.Where(a => !a.Name.StartsWith("System."))
					.OrderBy(a => a.Name))
				{
					details.AppendLine($"{assemblyName.Name} {assemblyName.Version}");
				}

				details.AppendLine();
				details.AppendLine("Exception message stack:");

				// We want the deepest exceptions first, as they are the most important.
				var exceptionStack = FlattenException(exception).Reverse().ToList();

				for (int i = 0; i < exceptionStack.Count; i++)
				{
					details.AppendLine($"{i + 1}. {exceptionStack[i]}");
				}

				details.AppendLine();

				if (additionalData != null)
				{
					details.AppendLine();
					details.AppendLine("Additional data:");
					details.AppendLine(Helpers.Debug.ToDebugString(additionalData));

					details.AppendLine();
				}

				log.Error(details.ToString());
			}
			catch (Exception ex)
			{
				log.Wtf("Exception occurred when attempting to create error report! " + ex);
			}
		}

		/// <summary>
		/// Exceptions may contain inner exceptions. This method transforms this
		/// hierarchical structure into the simple collection of exceptions.
		/// </summary>
		private static IEnumerable<Exception> FlattenException(Exception ex)
		{
			var currentException = ex;

			do
			{
				yield return currentException;

				currentException = currentException.InnerException;
			} while (currentException != null);
		}

		private const int MaxSummaryLength = 1000;

		private static string Summarize(Exception exception)
		{
			const string replaceSequence = ", ";

			var rootCause = FlattenException(exception).Last();

			var subjectLine = (rootCause.Message ?? "")
				// The order is very important. Make sure that it's correct.
				.Replace("\r\n", replaceSequence)
				.Replace("\n", replaceSequence)
				.Replace("\r", replaceSequence);

			// Let's limit the length - it's supposed to be a summary, after all!
			if (subjectLine.Length > MaxSummaryLength)
				subjectLine = subjectLine.Substring(0, MaxSummaryLength);

			return subjectLine;
		}
	}
}