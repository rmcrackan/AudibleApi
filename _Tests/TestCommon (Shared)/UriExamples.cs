using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

// Uri stuff is way too nuanced to keep in my head
namespace UriExamples
{
	[TestClass]
	public class UriTesting
	{
		Uri http = new Uri("http://www.a.com");
		Uri https = new Uri("https://www.a.com");
		Uri slash = new Uri("http://www.a.com/");
		Uri fragment = new Uri("http://www.a.com/#z");
		Uri param = new Uri("http://www.a.com/?z=1");

		[TestMethod]
		public void compare_equality()
		{
			// http, slash, fragment -- the same for all tests except AbsoluteUri

			Assert.IsFalse(http == https);
			Assert.IsTrue(http == slash); // true
			Assert.IsTrue(http == fragment); // true
			Assert.IsFalse(http == param);
			Assert.IsFalse(https == slash);
			Assert.IsFalse(https == fragment);
			Assert.IsFalse(https == param);
			Assert.IsTrue(slash == fragment); // true
			Assert.IsFalse(https == param);
			Assert.IsFalse(fragment == param);

			Assert.IsFalse(http.Equals(https));
			Assert.IsTrue(http.Equals(slash)); // true
			Assert.IsTrue(http.Equals(fragment)); // true
			Assert.IsFalse(http.Equals(param));
			Assert.IsFalse(https.Equals(slash));
			Assert.IsFalse(https.Equals(fragment));
			Assert.IsFalse(https.Equals(param));
			Assert.IsTrue(slash.Equals(fragment)); // true
			Assert.IsFalse(https.Equals(param));
			Assert.IsFalse(fragment.Equals(param));

			Assert.IsFalse(http.AbsoluteUri == https.AbsoluteUri);
			Assert.IsTrue(http.AbsoluteUri == slash.AbsoluteUri); // true
			Assert.IsFalse(http.AbsoluteUri == fragment.AbsoluteUri);
			Assert.IsFalse(http.AbsoluteUri == param.AbsoluteUri);
			Assert.IsFalse(https.AbsoluteUri == slash.AbsoluteUri);
			Assert.IsFalse(https.AbsoluteUri == fragment.AbsoluteUri);
			Assert.IsFalse(https.AbsoluteUri == param.AbsoluteUri);
			Assert.IsFalse(slash.AbsoluteUri == fragment.AbsoluteUri);
			Assert.IsFalse(https.AbsoluteUri == param.AbsoluteUri);
			Assert.IsFalse(fragment.AbsoluteUri == param.AbsoluteUri);
		}

		[TestMethod]
		public void dictionary_hashing()
		{
			var dic = new Dictionary<Uri, string>();

			void dicTest(Uri uri, bool shouldAdd)
			{
				var preCount = dic.Count;
				dic[uri] = "";
				var postCount = dic.Count;

				var didAdd = preCount != postCount;
				Assert.AreEqual(shouldAdd, didAdd);
			}

			dicTest(http, true);
			dicTest(https, true);
			dicTest(slash, false);
			dicTest(fragment, false);
			dicTest(param, true);
		}

		[TestMethod]
		public void combine_Uri_and_string()
		{
			new Uri(slash, "/a/z.html?f=b").AbsoluteUri.Should()
				.Be("http://www.a.com/a/z.html?f=b");
			new Uri(slash, "/a/z.html?f=b").AbsoluteUri.Should()
				.NotBe("http://www.a.com//a/z.html?f=b");
		}

		[TestMethod]
		public void host()
		{
			var u = "https://a.b.com";
			var uri = new Uri(u);

			uri.Host.Should().Be("a.b.com");
		}

		[TestMethod]
		public void AbsoluteUri_ends_in_slash()
			=> new Uri("https://a.b.com").AbsoluteUri.Should().Be("https://a.b.com/");
	}
}
