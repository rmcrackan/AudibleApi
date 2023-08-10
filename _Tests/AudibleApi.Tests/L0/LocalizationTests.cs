namespace LocalizationTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void loads_json_file()
		{
			var us = Localization.Get("us");

			us.LoginDomain().Should().Be("amazon");
			us.CountryCode.Should().Be("us");
			us.TopDomain.Should().Be("com");
			us.MarketPlaceId.Should().Be("AF2M0KC94RCEA");
			us.Language.Should().Be("en-US");
		}
	}
}
