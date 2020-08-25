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

namespace LocalizationTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void loads_json_file()
		{
			var us = Localization.Get("us");

			us.LoginDomain.Should().Be("amazon");
			us.CountryCode.Should().Be("us");
			us.TopDomain.Should().Be("com");
			us.MarketPlaceId.Should().Be("AF2M0KC94RCEA");
			us.Language.Should().Be("en-US");
		}
	}
}
