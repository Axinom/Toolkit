namespace Tests
{
    using Axinom.Toolkit;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public sealed class HttpResponseMessageExtensionsTests : BaseTestClass
    {
        [TestMethod]
        public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithOkResponse_PassesVerification()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            await response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync();
        }

        [TestMethod]
        public async Task EnsureSuccessStatusCodeAndReportFailureDetailsAsync_WithNotFoundResponse_FailsVerification()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => response.EnsureSuccessStatusCodeAndReportFailureDetailsAsync());
        }

        [TestMethod]
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

                Assert.IsTrue(ex.Message.Contains(canary));
            }
        }
    }
}