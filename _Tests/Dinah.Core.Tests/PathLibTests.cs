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
using TestCommon;

namespace PathLibTests
{
	[TestClass]
	public class GetPathWithExtensionFromAnotherFile
	{
		[TestMethod]
		public void null_params_throw()
		{
			Assert.ThrowsException<ArgumentNullException>(() => PathLib.GetPathWithExtensionFromAnotherFile(null, "foo"));
			Assert.ThrowsException<ArgumentNullException>(() => PathLib.GetPathWithExtensionFromAnotherFile("foo", null));
		}

		[TestMethod]
		public void blank_params_throw()
		{
			Assert.ThrowsException<ArgumentException>(() => PathLib.GetPathWithExtensionFromAnotherFile("", "foo"));
			Assert.ThrowsException<ArgumentException>(() => PathLib.GetPathWithExtensionFromAnotherFile("   ", "foo"));
			Assert.ThrowsException<ArgumentException>(() => PathLib.GetPathWithExtensionFromAnotherFile("foo", ""));
			Assert.ThrowsException<ArgumentException>(() => PathLib.GetPathWithExtensionFromAnotherFile("foo", "   "));
		}

		[TestMethod]
		[DataRow("http://test.com/a/b/c")]
		[DataRow("http://test.com/a/b/c?a=1&b=2")]
		[DataRow("foo")]
		[DataRow(@"c:\foo\bar\xyz")]
		public void downloadLink_no_extension_throws(string link)
			=> Assert.ThrowsException<FormatException>(() => PathLib.GetPathWithExtensionFromAnotherFile("foo", link));

		[TestMethod]
		// desiredPath no extension
		[DataRow(@"C:\foo\bar", "http://test.com/a/b/c.xyz?a=1&b=z", @"C:\foo\bar.xyz")]
		// extension not changed
		[DataRow(@"C:\foo\bar.xyz", "http://test.com/a/b/c.xyz?a=1&b=z", @"C:\foo\bar.xyz")]
		// extension changed
		[DataRow(@"C:\foo\bar.txt", "http://test.com/a/b/c.xyz?a=1&b=z", @"C:\foo\bar.xyz")]
		[DataRow(@"C:\foo\bar.txt", "abc.xyz", @"C:\foo\bar.xyz")]
		[DataRow(@"C:\foo\bar.txt", @"\abc.xyz", @"C:\foo\bar.xyz")]
		[DataRow(@"C:\foo\bar.txt", @"foo\abc.xyz", @"C:\foo\bar.xyz")]
		[DataRow(@"C:\foo\bar.txt", @"C:\foo\abc.xyz", @"C:\foo\bar.xyz")]
		public void success(
			string keepPathAndName,
			string keepExt,
			string expected)
			=> PathLib.GetPathWithExtensionFromAnotherFile(keepPathAndName, keepExt)
				.Should().Be(expected);
	}

}
