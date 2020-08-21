using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;

namespace ResourcesTests
{
	[TestClass]
	public class strings
	{
		[TestMethod]
		public void verify_all_uk()
		{
			var uk = Localization.Get("uk");

			uk.CountryCode.Should().Be("uk");
			uk.AudibleApiUri().ToString().Should().Be("https://api.audible.co.uk/");
			uk.AmazonApiUri().ToString().Should().Be("https://api.amazon.co.uk/");
			uk.AmazonLoginUri().ToString().Should().Be("https://www.amazon.co.uk/");

			var oauthExpected = @"
https://www.amazon.co.uk/ap/signin?openid.identity=http%3a%2f%2fspecs.openid.net%2fauth%2f2.0%2fidentifier_select&openid.claimed_id=http%3a%2f%2fspecs.openid.net%2fauth%2f2.0%2fidentifier_select&openid.ns.oa2=http%3a%2f%2fwww.amazon.com%2fap%2fext%2foauth%2f2&openid.ns.pape=http%3a%2f%2fspecs.openid.net%2fextensions%2fpape%2f1.0&openid.ns=http%3a%2f%2fspecs.openid.net%2fauth%2f2.0&openid.oa2.response_type=token&openid.return_to=https%3a%2f%2fwww.amazon.co.uk%2fap%2fmaplanding&openid.assoc_handle=amzn_audible_ios_uk&pageId=amzn_audible_ios&accountStatusPolicy=P1&openid.mode=checkid_setup&openid.oa2.client_id=device%3a6a52316c62706d53427a5735505a76477a45375959566674327959465a6374424a53497069546d45234132435a4a5a474c4b324a4a564d&language=en-GB&marketPlaceId=A2I9A3Q2GNFNGQ&openid.oa2.scope=device_auth_access&forceMobileLayout=true&openid.pape.max_auth_age=0
".Trim();
			uk.OAuthUrl().Should().Be(oauthExpected);
			uk.RegisterDomain().Should().Be(".amazon.co.uk");
			uk.LanguageTag().Should().Be("en-GB");
		}
	}
}
