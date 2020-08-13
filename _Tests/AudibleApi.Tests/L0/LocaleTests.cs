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
		[DataRow(null, "cc", "dd", "mp", "ll")]
		[DataRow("nn", null, "dd", "mp", "ll")]
		[DataRow("nn", "cc", null, "mp", "ll")]
		[DataRow("nn", "cc", "dd", null, "ll")]
		[DataRow("nn", "cc", "dd", "mp", null)]
		public void null_param_throws(string name, string countryCode, string domain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentNullException>(() => new Locale(name, countryCode, domain, marketPlaceId, language));

		[TestMethod]
		[DataRow("", "cc", "dd", "mp", "ll")]
		[DataRow("   ", "cc", "dd", "mp", "ll")]
		[DataRow("nn", "", "dd", "mp", "ll")]
		[DataRow("nn", "   ", "dd", "mp", "ll")]
		[DataRow("nn", "cc", "", "mp", "ll")]
		[DataRow("nn", "cc", "   ", "mp", "ll")]
		[DataRow("nn", "cc", "dd", "", "ll")]
		[DataRow("nn", "cc", "dd", "   ", "ll")]
		[DataRow("nn", "cc", "dd", "mp", "")]
		[DataRow("nn", "cc", "dd", "mp", "   ")]
		public void blank_param_throws(string name, string countryCode, string domain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentException>(() => new Locale(name, countryCode, domain, marketPlaceId, language));

		[TestMethod]
		public void instances_are_equal()
		{
			var locale1 = new Locale("nn", "cc", "dd", "mp", "ll");
			var locale2 = new Locale("nn", "cc", "dd", "mp", "ll");

			Assert.IsTrue(locale1 == locale2);
			Assert.IsTrue(locale1.Equals(locale2));
		}
	}
}
