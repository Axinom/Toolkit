namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Windows.Web.Http;

	public static class ExtensionsForUwpHttpResponseMessage
	{
		/// <summary>
		/// Returns the status string describing the response; this includes the status code and the reason phrase.
		/// </summary>
		public static string GetStatusLine(this HttpResponseMessage response)
		{
			Helpers.Argument.ValidateIsNotNull(response, "response");

			return string.Format("{0} {1}", (int)response.StatusCode, response.ReasonPhrase);
		}

		/// <summary>
		/// Ensures that the response message has a response code indicating success.
		/// On failure, throws an exception that contains detailed information about the response message.
		/// </summary>
		public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeAndReportFailureDetailsAsync(this HttpResponseMessage response)
		{
			Helpers.Argument.ValidateIsNotNull(response, "response");

			if (response.IsSuccessStatusCode)
				return response;

			StringBuilder details = new StringBuilder();
			details.Append("StatusCode: ");
			details.Append((int)response.StatusCode);
			details.Append(", ReasonPhrase: '");
			details.Append(response.ReasonPhrase);
			details.Append("', Version: ");
			details.Append(response.Version);
			details.Append(", Headers:\r\n");
			details.Append(DumpHeaders(response.Headers));

			if (response?.Content != null)
				details.Append(DumpHeaders(response.Content.Headers));

			if (response.Content == null)
			{
				details.Append(", Content: <null>");
			}
			else
			{
				try
				{
					var responseBody = await response.Content.ReadAsStringAsync().IgnoreContext();
					details.Append(", Content: ");
					details.Append(responseBody);
				}
				catch (Exception ex)
				{
					details.AppendFormat(", Content not available: {0}", ex.Message);
				}
			}

			throw new EnvironmentException("Response status code does not indicate success. " + details);
		}

		internal static string DumpHeaders(IDictionary<string, string> headers)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{\r\n");

			foreach (var pair in headers)
			{
				builder.Append("  ");
				builder.Append(pair.Key);
				builder.Append(": ");
				builder.Append(pair.Value);
				builder.Append("\r\n");
			}
			builder.Append('}');
			return builder.ToString();
		}
	}
}