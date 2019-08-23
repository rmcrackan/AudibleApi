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
using TestCommon;

namespace Authoriz.RefreshTokenTests
{
	[TestClass]
	public class ValidateInput
	{
		[TestMethod]
		public void null_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new RefreshToken(null));

		[TestMethod]
		public void blank_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken(""));
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken("   "));
		}

		[TestMethod]
		public void bad_beginning()
		{
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken("foo"));
		}

		[TestMethod]
		public void valid_strings()
		{
			var justBeginning = "Atnr|";
			new RefreshToken(justBeginning)
				.Value.Should().Be(justBeginning);

			var full = "Atnr|foo";
			new RefreshToken(full)
				.Value.Should().Be(full);
		}
	}
}
