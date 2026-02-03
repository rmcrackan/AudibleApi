namespace ResourcesTests;

[TestClass]
public class strings
{
	[TestMethod]
	public void verify_all_uk()
	{
		var uk = Localization.Get("uk");

		uk.CountryCode.ShouldBe("uk");
		uk.AudibleApiUri().ToString().ShouldBe("https://api.audible.co.uk/");
		uk.AmazonApiUri().ToString().ShouldBe("https://api.amazon.co.uk/");
		uk.LoginUri().ToString().ShouldBe("https://www.amazon.co.uk/");
		uk.RegisterDomain().ShouldBe(".amazon.co.uk");
		uk.Language.ShouldBe("en-GB");
	}
}
