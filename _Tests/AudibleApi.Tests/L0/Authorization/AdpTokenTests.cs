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

namespace Authoriz.AdpTokenTests
{
	[TestClass]
	public class ValidateInput
	{
		[TestMethod]
		public void null_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new AdpToken(null));

		[TestMethod]
		public void blank_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken(""));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("   "));
		}

		[TestMethod]
		public void bad_format()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("no braces"));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{no end brace"));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("no begin end}"));
		}

		[TestMethod]
		public void missing_entry()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}"));
		}

		[TestMethod]
		public void extra_entry()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:}{foo:bar}"));
		}

		[TestMethod]
		public void bad_name()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:foo}{serial:}"));
		}

		[TestMethod]
		public void valid_strings()
		{
			var min = "{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:}";
			new AdpToken(min)
				.Value.Should().Be(min);

			var better = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
			new AdpToken(better)
				.Value.Should().Be(better);
		}

		[TestMethod]
		public void test_parser()
		{
			var enc = "asdfghjkl==";
			var key = "123456789==";
			var iv = "1a2s3d4==";
			var name = "QURQVG9rZW5FbmNyeXB0aW9uS2V5";
			var serial = "Mg==";
			var str
				= $"{{enc:{enc}}}"
				+ $"{{key:{key}}}"
				+ $"{{iv:{iv}}}"
				+ $"{{name:{name}}}"
				+ $"{{serial:{serial}}}";

			var dic = AdpToken.adp_parser.Parse(str);

			dic.Count.Should().Be(5);
			dic.ContainsKey("enc").Should().BeTrue();
			dic.ContainsKey("key").Should().BeTrue();
			dic.ContainsKey("iv").Should().BeTrue();
			dic.ContainsKey("name").Should().BeTrue();
			dic.ContainsKey("serial").Should().BeTrue();
			dic["enc"].Should().Be(enc);
			dic["key"].Should().Be(key);
			dic["iv"].Should().Be(iv);
			dic["name"].Should().Be(name);
			dic["serial"].Should().Be(serial);
		}
	}
}
