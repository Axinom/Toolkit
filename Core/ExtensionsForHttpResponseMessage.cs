namespace Axinom.Toolkit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading.Tasks;

	public static class ExtensionsForHttpResponseMessage
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
			details.Append(response.ReasonPhrase ?? "<null>");
			details.Append("', Version: ");
			details.Append(response.Version);
			details.Append(", Headers:\r\n");
			details.Append(DumpHeaders(new HttpHeaders[] { response.Headers, (response.Content == null) ? null : response.Content.Headers }));

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

			throw new HttpRequestException("Response status code does not indicate success. " + details);
		}

		internal static string DumpHeaders(params HttpHeaders[] headers)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{\r\n");
			foreach (HttpHeaders header in headers)
			{
				if (header != null)
				{
					foreach (KeyValuePair<string, IEnumerable<string>> pair in header)
					{
						foreach (string str in pair.Value)
						{
							builder.Append("  ");
							builder.Append(pair.Key);
							builder.Append(": ");
							builder.Append(str);
							builder.Append("\r\n");
						}
					}
				}
			}
			builder.Append('}');
			return builder.ToString();
		}
	}
}