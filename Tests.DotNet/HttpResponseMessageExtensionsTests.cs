namespace Tests
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Axinom.Toolkit;
	using Xunit;

	public sealed class HttpResponseMessageExtensionsTests
	{
		[Fact]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithOkResponse_PassesVerification()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();
		}

		[Fact]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithNotFoundResponse_FailsVerification()
		{
			var response = new HttpResponseMessage(HttpStatusCode.NotFound);

			await Assert.ThrowsAsync<HttpRequestException>(() => response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync());
		}

		[Fact]
		public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithNotFoundResponse_ProvidesStringContentInDetails()
		{
			const string canary = "gioiiiiiiiiiiooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo";

			var response = new HttpResponseMessage(HttpStatusCode.NotFound);
			response.Content = new StringContent(canary);

			try
			{
				await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();

				throw new Exception("Expected exception to be thrown here.");
			}
			catch (HttpRequestException ex)
			{
				Log.Default.Debug(ex.Message);

				Assert.Contains(canary, ex.Message);
			}
		}
	}
}