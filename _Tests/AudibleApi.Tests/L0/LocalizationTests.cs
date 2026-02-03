namespace LocalizationTests;

[TestClass]
public class ctor
{
	[TestMethod]
	public void loads_json_file()
	{
		var us = Localization.Get("us");

		us.LoginDomain().ShouldBe("amazon");
		us.CountryCode.ShouldBe("us");
		us.TopDomain.ShouldBe("com");
		us.MarketPlaceId.ShouldBe("AF2M0KC94RCEA");
		us.Language.ShouldBe("en-US");
	}
}
