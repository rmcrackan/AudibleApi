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

namespace LocaleTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		[DataRow(null, "ld", "cc", "td", "mp", "ll")]
		[DataRow("nn", null, "cc", "td", "mp", "ll")]
		[DataRow("nn", "ld", null, "td", "mp", "ll")]
		[DataRow("nn", "ld", "cc", null, "mp", "ll")]
		[DataRow("nn", "ld", "cc", "td", null, "ll")]
		[DataRow("nn", "ld", "cc", "td", "mp", null)]
		public void null_param_throws(string name, string loginDomain, string countryCode, string topDomain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentNullException>(() => new Locale(name, loginDomain, countryCode, topDomain, marketPlaceId, language));

		[TestMethod]
		[DataRow("", "ld", "cc", "td", "mp", "ll")]
		[DataRow("   ", "ld", "cc", "td", "mp", "ll")]
		[DataRow("nn", "", "cc", "td", "mp", "ll")]
		[DataRow("nn", "   ", "cc", "td", "mp", "ll")]
		[DataRow("nn", "ld", "", "td", "mp", "ll")]
		[DataRow("nn", "ld", "   ", "td", "mp", "ll")]
		[DataRow("nn", "ld", "cc", "", "mp", "ll")]
		[DataRow("nn", "ld", "cc", "   ", "mp", "ll")]
		[DataRow("nn", "ld", "cc", "td", "", "ll")]
		[DataRow("nn", "ld", "cc", "td", "   ", "ll")]
		[DataRow("nn", "ld", "cc", "td", "mp", "")]
		[DataRow("nn", "ld", "cc", "td", "mp", "   ")]
		public void blank_param_throws(string name, string loginDomain, string countryCode, string topDomain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentException>(() => new Locale(name, loginDomain, countryCode, topDomain, marketPlaceId, language));

		[TestMethod]
		public void instances_are_equal()
		{
			var locale1 = new Locale("nn", "ld", "cc", "td", "mp", "ll");
			var locale2 = new Locale("nn", "ld", "cc", "td", "mp", "ll");

			Assert.IsTrue(locale1 == locale2);
			Assert.IsTrue(locale1.Equals(locale2));
		}
	}

	[TestClass]
	public class Empty
	{
		[TestMethod]
		public void ensure_valid()
		{
			// most important part is to get past this line w/o exceptions
			var l = Locale.Empty;
			l.Name.Should().Be("[empty]");
			l.CountryCode.Should().BeNull();
		}
	}
}
