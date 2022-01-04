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
			uk.LoginUri().ToString().Should().Be("https://www.amazon.co.uk/");
			uk.RegisterDomain().Should().Be(".amazon.co.uk");
			uk.Language.Should().Be("en-GB");
		}
	}
}
