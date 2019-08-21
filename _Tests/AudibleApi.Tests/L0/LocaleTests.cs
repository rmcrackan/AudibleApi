using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using BaseLib;
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
		[DataRow(null, "dd", "mp", "ll")]
		[DataRow("cc", null, "mp", "ll")]
		[DataRow("cc", "dd", null, "ll")]
		[DataRow("cc", "dd", "mp", null)]
		public void null_param_throws(string countryCode, string domain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentNullException>(() => new Locale(countryCode, domain, marketPlaceId, language));

		[TestMethod]
		[DataRow("", "dd", "mp", "ll")]
		[DataRow("   ", "dd", "mp", "ll")]
		[DataRow("cc", "", "mp", "ll")]
		[DataRow("cc", "   ", "mp", "ll")]
		[DataRow("cc", "dd", "", "ll")]
		[DataRow("cc", "dd", "   ", "ll")]
		[DataRow("cc", "dd", "mp", "")]
		[DataRow("cc", "dd", "mp", "   ")]
		public void blank_param_throws(string countryCode, string domain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentException>(() => new Locale(countryCode, domain, marketPlaceId, language));

		[TestMethod]
		public void instances_are_equal()
		{
			var locale1 = new Locale("cc", "dd", "mp", "ll");
			var locale2 = new Locale("cc", "dd", "mp", "ll");

			Assert.IsTrue(locale1 == locale2);
			Assert.IsTrue(locale1.Equals(locale2));
		}
	}
}
