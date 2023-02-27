using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
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
			Assert.ThrowsException<FormatException>(() => new PrivateKey(justBeginningAndEnd)
				.Value.Should().Be(justBeginningAndEnd));

			var withWhitespace = "\r\n  -----BEGIN RSA PRIVATE KEY-----END RSA PRIVATE KEY-----\r\n  ";
			Assert.ThrowsException<FormatException>(() => new PrivateKey(withWhitespace)
				.Value.Should().Be(withWhitespace));

			var full = @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpgIBAAKCAQEA5nPbGSVDmlEH2tJa6kz/P2HI8IeirhfPHdmi+X/nsb9i3WNf
tmEdZxfK26IValQDXvBH17a1gr0HD6pYse1XsV2w0HxiW1RW+ZnjL8/fzPdkSOb+
4xKlqRopCueBSdDGgAF06spZ3IeHLfEFOJX4dO1Y73pFBUkA0k53LT12L2Tjay/r
buZHJqIzxmwja7/nkiWL0Xo7UySHtQACYsKEatu6yHBS+cPTlGR/qeUpeJTHwDLP
7ZQ7kWzJGY1mfInYekjlZLsMsWswso3pg1vPyHgxzM2BWhY8m6mlXQ9G/USxBTib
MNuMtpR73XsgamneFCc+Uv1cxw7ofZ41YOOAbQIDAQABAoIBAQDIre8HkKm0Aggj
B7df/TjxCsgenR6PF/Cmf9UqC7XJ1W3UeCrq+NrP4aonZJfdhdeBnyAQuuyJMu6p
N6ARISuSKpJEm2xTN7idluJ9yjmLlYtg6LbhKmXUQhGniz3M999DrQERTLDAF80h
tpbjVcWMnPsrX4AnQBFVEjs5zCHU1hD+X463EmUHBWyT975jbZ8Fy7/fTzkdzLnn
qE5lROALr2MCAAwQRFbRE6dd52vnXaBrVcAtRzjATts3WG3+SNi2Fm/OrYqQcY9e
lBexNviT8VcldOAMrO10E2u0d+tvxFzwB3ABMvaVamrEZky4XSfB6aLzpD0JJj1s
UHnIiVwJAoGBAPl8nLll/J9rud/N2HiAX2YkP0MC0HW4yM3KxLtXKyXrP5qBpaci
wTDUmSWEEE3GUJMM1Z4d9tl9Lz2MhU2KqkEvLI3kQ7aUu33PYUBGMVcUzhFQ49lU
Nzz8YB183iqo31o/DKk2Cr5gI7SykQZ0gn/urZkEJeErLzlhPXcyeY5jAoGBAOx4
CGucVdv5MbdXZP8jVzxuvUlSp7BIQJ2phQXDFBNApFKnZn7yBYBx7dqzleymGm+R
INZAurg3SNw4nvbQc3Z2dJ8I+n5ErjFCKp1IedVxx1eMEfecTwrQZuUwLISIyjqF
czSJNwcNqzCx67z397/Cg5K/0pu6uIe0r7xozcbvAoGBAOOvZ9CDVPOg+rdXQvFm
Jqou9lUPonNtOkUlgjl+qfAnK5q0KxvHSgxoWYO1bLOuAybQlbuBmSCPcKd5MMa9
f/eRN9YetfVQ83Mz6YshBDJ22EFRUz+p7eeIY6dFp/PCvmO8Gq/qlA996dglBtmf
RuG+T0vQT0mZgbWaGuBHfkwFAoGBAMOLg1MRxgKRMKavk6pU3EfyP3+J5XemWCDI
1WLtbgV5uClNmzmxBBGypQHs7jbzKPtHpULn5kB+HzdVb0clG8ZDsK7u6s5OF0pO
sBS+oVl7rF/eSeFcFhUYP26ZhsbWo3z/bERuj926VO2AxDPRTsP5o3pQPGZhY0V9
irGgbUJrAoGBAOseS3J4BqYM4R3Hr7cRAhvzSjIkeTcDF1zTOa4FZDHBxZ6g2PNq
8ekhtfn1zPczsPTF1vNuqEISKLxaPkVPiw0mtaZQjVwpF/IOxMNjWVLp6oJf8Mm2
BxlXqPnQ4mG66oqSFQgDEmFdMhRb2of6xL1gYYL62C80G2T7QtmPfSab
-----END RSA PRIVATE KEY-----
";
			new PrivateKey(full)
				.Value.Should().Be(full);
		}
	}
}
