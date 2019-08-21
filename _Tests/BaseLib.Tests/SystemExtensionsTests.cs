using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib;
using FluentAssertions;

namespace SystemExtensionsTests
{
    [TestClass]
    public class ToUnixTimeStamp
    {
        [TestMethod]
        public void should_pass()
        {
            should_pass(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0L);
            should_pass(new DateTime(2019, 1, 1, 12, 30, 30, 0, DateTimeKind.Utc), 1546345830L);
        }
        void should_pass(DateTime dateTime, long unixTimeStamp) => Assert.AreEqual(dateTime.ToUnixTimeStamp(), unixTimeStamp);
    }

    [TestClass]
    public class ToRfc3339String
    {
        [TestMethod]
        public void should_pass()
        {
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .ToRfc3339String()
                .Should().Be("1970-01-01T00:00:00Z");
            new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .ToRfc3339String()
                .Should().Be("2000-01-01T00:00:00Z");
            new DateTime(2019, 2, 3, 4, 5, 6, 7, DateTimeKind.Utc)
                .ToRfc3339String()
                .Should().Be("2019-02-03T04:05:06.007Z");
        }
    }

	[TestClass]
	public class GetOrigin
	{
		[TestMethod]
		[DataRow("http://www.a.com", "http://www.a.com")]
		[DataRow("https://www.a.com", "https://www.a.com")]
		[DataRow("http://www.a.com/", "http://www.a.com")]
		[DataRow("http://www.a.com/#z", "http://www.a.com")]
		[DataRow("http://www.a.com/?z=1", "http://www.a.com")]
		[DataRow("http://www.a.com:1234", "http://www.a.com:1234")]
		[DataRow("https://www.a.com:1234", "https://www.a.com:1234")]
		[DataRow("http://www.a.com:1234/", "http://www.a.com:1234")]
		[DataRow("http://www.a.com:1234/#z", "http://www.a.com:1234")]
		[DataRow("http://www.a.com:1234/?z=1", "http://www.a.com:1234")]
		public void extract_origin(string url, string expected)
			=> new Uri(url).GetOrigin().Should().Be(expected);
	}
}
