using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TestAudibleApiCommon;
using TestCommon;

namespace Authoriz.AccessTokenTests
{
    [TestClass]
    public class ctor
	{
		[TestMethod]
		public void null_value_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new AccessToken(null, DateTime.MinValue));

		[TestMethod]
        public void blank_string_throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new AccessToken("", DateTime.MinValue));
            Assert.ThrowsException<ArgumentException>(() => new AccessToken("   ", DateTime.MinValue));
        }

		[TestMethod]
		public void bad_beginning()
		{
			Assert.ThrowsException<ArgumentException>(() => new AccessToken("foo", DateTime.MinValue));
		}

		[TestMethod]
		public void valid_strings()
		{
			var justBeginning = "Atna|";
			new AccessToken(justBeginning, DateTime.MinValue)
				.TokenValue.Should().Be(justBeginning);

			var full = "Atna|foo";
			new AccessToken(full, DateTime.MinValue)
				.TokenValue.Should().Be(full);
		}
	}

    [TestClass]
    public class Invalidate
    {
        [TestMethod]
        public void invalidate_token()
        {
            var token = new AccessToken("Atna|foo", DateTime.MaxValue);
            token.Invalidate();
            token.Expires.Should().Be(DateTime.MinValue);
        }
    }

	[TestClass]
	public class ToString
	{
		[TestMethod]
		public void print()
		{
			new AccessToken("Atna|foo", DateTime.MaxValue)
				.ToString()
				.Should().Be(
				"AccessToken. Value=Atna|foo. Expires=12/31/9999 11:59:59 PM"
				);
		}
	}
}
