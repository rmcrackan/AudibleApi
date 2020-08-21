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
using TestCommon;

namespace Authentic.LoginResultTests
{
    internal class ValidateLoginResult : LoginResult
    {
        public ValidateLoginResult(ApiHttpClient client, ISystemDateTime systemDateTime, Locale locale, string responseBody) : base(client, systemDateTime, locale, responseBody) { }
    }

    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_client_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(null, StaticSystemDateTime.Past, Locales.Us, "foo"));

		[TestMethod]
		public void null_systemDateTime_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(ApiHttpClientMock.GetClient(), null, Locales.Us, "foo"));

        [TestMethod]
        public void null_locale_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, null, "foo"));

        [TestMethod]
        public void null_responseBody_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, Locales.Us, null));

        [TestMethod]
        public void inputs_are_saved()
        {
            var body
                = "<input name='a' value='b' />"
                + "<input name='y' value='z' />";
            var result = new ValidateLoginResult(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, Locales.Us, body);
            var inputs = result.GetInputsReadOnly();
            inputs.Count.Should().Be(2);
            inputs["a"].Should().Be("b");
            inputs["y"].Should().Be("z");
        }
    }
}
