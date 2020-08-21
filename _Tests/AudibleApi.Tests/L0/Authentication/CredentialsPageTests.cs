using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authentic.CredentialsPageTests
{
    [TestClass]
    public class SubmitAsync
    {
        private CredentialsPage getPage()
			=> new CredentialsPage(
				ApiHttpClientMock.GetClient(),
				StaticSystemDateTime.Past,
                Locales.Us,
                "body");

		[TestMethod]
        public async Task null_email_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync(null, "pw"));

        [TestMethod]
        public async Task blank_email_throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("", "pw"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("   ", "pw"));
        }

        [TestMethod]
        public async Task null_password_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync("email", null));

        [TestMethod]
        public async Task blank_password_throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("email", ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("email", "    "));
		}

		[TestMethod]
        public async Task valid_param_calls_GetResultsPageAsync()
        {
            var responseToCaptureRequest = new HttpResponseMessage();

			var page = new CredentialsPage(
				ApiHttpClientMock.GetClient(responseToCaptureRequest),
				StaticSystemDateTime.Past,
                Locales.Us,
                "body");
			await Assert.ThrowsExceptionAsync<LoginFailedException>(() => page.SubmitAsync("e", "pw"));

            var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
            var split = content.Split('&');
            var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
            dic.Count.Should().Be(3);
            dic["email"].Should().Be("e");
            dic["password"].Should().Be("pw");
            dic["metadata1"].Length.Should().BeGreaterThan(100);
        }
    }

	[TestClass]
	public class GenerateMetadata
	{
		[TestMethod]
		public void validate_output()
		{
			var metadata = CredentialsPage.GenerateMetadata(Locales.Us, 123456789L);
			metadata.Should().Be(CredentialsPageMetadataPlaintext);
		}
	}
}
