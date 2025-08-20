namespace LocaleTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		[DataRow(null, "cc", "td", "mp", "ll")]
		[DataRow("nn", null, "td", "mp", "ll")]
		[DataRow("nn", "cc", null, "mp", "ll")]
		[DataRow("nn", "cc", "td", null, "ll")]
		[DataRow("nn", "cc", "td", "mp", null)]
		public void null_param_throws(string name, string countryCode, string topDomain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentNullException>(() => new Locale(name, countryCode, topDomain, marketPlaceId, language));

		[TestMethod]
		[DataRow("", "cc", "td", "mp", "ll")]
		[DataRow("   ", "cc", "td", "mp", "ll")]
		[DataRow("nn", "", "td", "mp", "ll")]
		[DataRow("nn", "   ", "td", "mp", "ll")]
		[DataRow("nn", "cc", "", "mp", "ll")]
		[DataRow("nn", "cc", "   ", "mp", "ll")]
		[DataRow("nn", "cc", "td", "", "ll")]
		[DataRow("nn", "cc", "td", "   ", "ll")]
		[DataRow("nn", "cc", "td", "mp", "")]
		[DataRow("nn", "cc", "td", "mp", "   ")]
		public void blank_param_throws(string name, string countryCode, string topDomain, string marketPlaceId, string language)
			=> Assert.ThrowsException<ArgumentException>(() => new Locale(name, countryCode, topDomain, marketPlaceId, language));

		[TestMethod]
		public void instances_are_equal()
		{
			var locale1 = new Locale("nn", "cc", "td", "mp", "ll", true);
			var locale2 = new Locale("nn", "cc", "td", "mp", "ll", true);

			Assert.IsTrue(locale1 == locale2);
			Assert.IsTrue(locale1.Equals(locale2));
		}

		[TestMethod]
		public void instances_are_not_equal()
		{
			var locale1 = new Locale("nn", "cc", "td", "mp", "ll", true);
			var locale2 = new Locale("nn", "cc", "td", "mp", "ll", false);

			Assert.IsFalse(locale1 == locale2);
			Assert.IsFalse(locale1.Equals(locale2));
		}
	}

	[TestClass]
	public class Empty
	{
		[TestMethod]
		public void ensure_valid()
		{
			// most important part is to get past this line w/o exceptions
			var l = Locale.Empty;
			l.Name.ShouldBe("[empty]");
			l.CountryCode.ShouldBeNull();
		}
	}
}
