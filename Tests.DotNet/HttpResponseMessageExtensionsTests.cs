namespace Tests.DotNet
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Axinom.Toolkit;
	using NUnit.Framework;

	[TestFixture]
	public sealed class HttpResponseMessageExtensionsTests
	{
		[Test]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithOkResponse_PassesVerification()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();
		}

		[Test]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithNotFoundResponse_FailsVerification()
		{
			var response = new HttpResponseMessage(HttpStatusCode.NotFound);
			await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();
		}

		[Test]
		[ExpectedException(typeof(HttpRequestException))]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithNotFoundResponse_ProvidesStringContentInDetails()
		{
			const string canary = "gioiiiiiiiiiiooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo";

			var response = new HttpResponseMessage(HttpStatusCode.NotFound);
			response.Content = new StringContent(canary);

			try
			{
				await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();
			}
			catch (HttpRequestException ex)
			{
				Trace.WriteLine(ex.Message);

				StringAssert.Contains(canary, ex.Message);

				throw;
			}
		}
	}
}