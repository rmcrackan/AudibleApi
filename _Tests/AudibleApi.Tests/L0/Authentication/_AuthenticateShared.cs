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
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;

namespace Authentic
{
	internal class AuthenticateShared
	{
		public static Authenticate GetAuthenticate(ApiHttpClient client)
			=> new Authenticate(Locales.Us, "", client, StaticSystemDateTime.Past)
			{
				MaxLoadSessionCookiesTrips = 3
			};
		public static Authenticate GetAuthenticate(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
			=> new Authenticate(Locales.Us, "", ApiHttpClientMock.GetClient(handlerReturnString, statusCode), StaticSystemDateTime.Past)
			{
				MaxLoadSessionCookiesTrips = 3
			};
		public static Authenticate GetAuthenticate(HttpResponseMessage response)
			=> new Authenticate(Locales.Us, "", ApiHttpClientMock.GetClient(response), StaticSystemDateTime.Past)
			{
				MaxLoadSessionCookiesTrips = 3
			};
	}
}
