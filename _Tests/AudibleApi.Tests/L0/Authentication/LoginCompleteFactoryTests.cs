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
using TestAudibleApiCommon;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authentic.ResultFactoryTests.LoginCompleteFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task null_returns_false()
            => (await ResultFactory.LoginComplete.IsMatchAsync(null)).Should().BeFalse();

        [TestMethod]
        public async Task _200_returns_false()
        {
            var code = (HttpStatusCode)200;
            var response = new HttpResponseMessage { StatusCode = code };
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task _400_returns_false()
        {
            var code = (HttpStatusCode)400;
            var response = new HttpResponseMessage { StatusCode = code };
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task null_Location_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            response.Headers.Location.Should().BeNull();
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task no_access_token_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved };
            response.Headers.Location = new Uri("http://t.co/?a=1");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task no_auth_time_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved };
            response.Headers.Location = new Uri("http://t.co/?openid.oa2.access_token=xyz");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task valid_returns_true()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved, Content = new StringContent("foo") };
            response.Headers.Location = new Uri("http://t.co/?openid.oa2.access_token=Atna|&openid.pape.auth_time=2000-01-01");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeTrue();
        }
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task valid_returns_LoginComplete()
        {
			var returnAccessToken = "Atna|";
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Moved,
				Content = new StringContent(AuthenticateResponse)
			};
            var uri = new Uri("http://test.com?openid.oa2.access_token="+ returnAccessToken + "&openid.pape.auth_time=2000-01-01%2000:00:01");
            response.Headers.Location = uri;

			// i can't this to work. it's not the point of this unit test so hardcode cookies below with CookieJar.Add()
            //response.Headers.Add("Set-Cookie", "session-id=139-1488065-0277455; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");
            //response.Headers.Add("Set-Cookie", "session-id-time=2193073634l; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");

			var apiClient = ApiClientMock.GetClient(response);
			var sysDateTime = StaticSystemDateTime.Past;
			var inputs = new Dictionary<string, string>();

			apiClient.CookieJar.Add(new Cookie { Name = "session-id", Value = "139-1488065-0277455", Domain = ".amazon.com", Expires = DateTime.Parse("Thu, 30-Jun-2039 19:07:14 GMT"), Path = "/" });
			apiClient.CookieJar.Add(new Cookie { Name = "session-id-time", Value = "2193073634l", Domain = ".amazon.com", Expires = DateTime.Parse("Thu, 30-Jun-2039 19:07:14 GMT"), Path = "/" });

			var loginComplete = await ResultFactory.LoginComplete.CreateResultAsync(
				apiClient,
				sysDateTime,
				response,
				inputs
				) as LoginComplete;

            var accessToken = new AccessToken(returnAccessToken, DateTime.Now.AddSeconds(1));
            loginComplete.Identity.ExistingAccessToken.TokenValue.Should().Be(accessToken.TokenValue);

            var collection = loginComplete.Identity.Cookies.ToKeyValuePair();
            collection.Count.Should().Be(2);

            var sessionIdCookie = collection.Single(c => c.Key == "session-id");
            sessionIdCookie.Key.Should().Be("session-id");
            sessionIdCookie.Value.Should().Be("139-1488065-0277455");

            var sessionIdTimeCookie = collection.Single(c => c.Key == "session-id-time");
            sessionIdTimeCookie.Key.Should().Be("session-id-time");
            sessionIdTimeCookie.Value.Should().Be("2193073634l");
        }
    }
}
