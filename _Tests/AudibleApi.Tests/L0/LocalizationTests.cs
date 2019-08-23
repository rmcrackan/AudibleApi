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
			=> Localization.CurrentLocale.Should().NotBeNull();

		[TestMethod]
		public void sets_us_default()
		{
			var curr = Localization.CurrentLocale;

			curr.CountryCode.Should().Be("us");
			curr.Domain.Should().Be("com");
			curr.MarketPlaceId.Should().Be("AF2M0KC94RCEA");
			curr.Language.Should().Be("en-US");
		}
	}

	[TestClass]
	public class SetLocale_LocaleName
	{
		[TestCleanup]
		public void reset_to_default()
			=> Localization.SetLocale(Localization.LocaleNames.US);

		[TestMethod]
		public void invalid_enum_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => Localization.SetLocale((Localization.LocaleNames)99));

		[TestMethod]
		public void valid_enum_sets()
		{
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "us");
			Localization.SetLocale(Localization.LocaleNames.Germany);
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "de");
		}
	}

	[TestClass]
	public class SetLocale_string
	{
		[TestCleanup]
		public void reset_to_default()
			=> Localization.SetLocale(Localization.LocaleNames.US);

		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => Localization.SetLocale(null));

		[TestMethod]
		public void blank_param_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => Localization.SetLocale(""));
			Assert.ThrowsException<ArgumentException>(() => Localization.SetLocale("   "));
		}

		[TestMethod]
		public void invalid_string_throws()
		{
			Assert.ThrowsException<KeyNotFoundException>(() => Localization.SetLocale("foo"));
		}

		[TestMethod]
		public void valid_string_sets()
		{
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "us");
			Localization.SetLocale("germany");
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "de");
		}

		[TestMethod]
		public void valid_string_with_whitespace()
		{
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "us");
			Localization.SetLocale("   germany  ");
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "de");
		}

		[TestMethod]
		public void valid_string_case_insensitive()
		{
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "us");
			Localization.SetLocale("   CaNaDa  ");
			Assert.AreEqual(Localization.CurrentLocale.CountryCode, "ca");
		}

		[TestMethod]
		public void verify_all_uk()
		{
			Localization.CurrentLocale.CountryCode.Should().Be("us");
			Localization.SetLocale(Localization.LocaleNames.UK);
			Localization.CurrentLocale.CountryCode.Should().Be("uk");

			var curr = Localization.CurrentLocale;

			curr.CountryCode.Should().Be("uk");
			curr.Domain.Should().Be("co.uk");
			curr.MarketPlaceId.Should().Be("A2I9A3Q2GNFNGQ");
			curr.Language.Should().Be("en-GB");
		}

		[TestMethod]
		public void ensure_no_caching()
		{
			verify_all_uk();
			Localization.CurrentLocale.CountryCode.Should().Be("uk");
			Localization.SetLocale(Localization.LocaleNames.US);
			Localization.CurrentLocale.CountryCode.Should().Be("us");

			var curr = Localization.CurrentLocale;

			curr.CountryCode.Should().Be("us");
			curr.Domain.Should().Be("com");
			curr.MarketPlaceId.Should().Be("AF2M0KC94RCEA");
			curr.Language.Should().Be("en-US");
		}
	}
}
