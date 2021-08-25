using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TestAudibleApiCommon;

namespace Authoriz.PrivateKeyTests
{
	[TestClass]
	public class ValidateInput
	{
		[TestMethod]
		public void null_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new PrivateKey(null));

		[TestMethod]
		public void blank_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new PrivateKey(""));
			Assert.ThrowsException<ArgumentException>(() => new PrivateKey("   "));
		}

		[TestMethod]
		public void bad_beginning()
		{
			Assert.ThrowsException<ArgumentException>(() => new PrivateKey("foo-----END RSA PRIVATE KEY-----"));
		}

		[TestMethod]
		public void bad_ending()
		{
			Assert.ThrowsException<ArgumentException>(() => new PrivateKey("-----BEGIN RSA PRIVATE KEY-----foo"));
		}

		[TestMethod]
		public void valid_strings()
		{
			var justBeginningAndEnd = "-----BEGIN RSA PRIVATE KEY-----END RSA PRIVATE KEY-----";
			new PrivateKey(justBeginningAndEnd)
				.Value.Should().Be(justBeginningAndEnd);

			var withWhitespace = "\r\n  -----BEGIN RSA PRIVATE KEY-----END RSA PRIVATE KEY-----\r\n  ";
			new PrivateKey(withWhitespace)
				.Value.Should().Be(withWhitespace);

			var full = @"
-----BEGIN RSA PRIVATE KEY-----
key stuff here
-----END RSA PRIVATE KEY-----
";
			new PrivateKey(full)
				.Value.Should().Be(full);
		}
	}
}
