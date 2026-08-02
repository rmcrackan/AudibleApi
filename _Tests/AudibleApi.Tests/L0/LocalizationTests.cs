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

	[TestMethod]
	public void get_by_country_code_prefers_modern_locale()
	{
		var de = Localization.Get("de");
		de.Name.ShouldBe("germany");
		de.CountryCode.ShouldBe("de");
		de.WithUsername.ShouldBeFalse();
		de.TopDomain.ShouldBe("de");

		var au = Localization.Get("au");
		au.Name.ShouldBe("australia");
		au.CountryCode.ShouldBe("au");

		var jp = Localization.Get("jp");
		jp.Name.ShouldBe("japan");
		jp.CountryCode.ShouldBe("jp");
	}

	[TestMethod]
	public void get_by_name_is_case_insensitive()
	{
		Localization.Get("Germany").Name.ShouldBe("germany");
		Localization.Get("DE").Name.ShouldBe("germany");
	}

	[TestMethod]
	public void get_unknown_returns_empty()
	{
		var empty = Localization.Get("not-a-locale");
		empty.ShouldBe(Locale.Empty);
	}

	[TestMethod]
	public void get_pre_amazon_name_still_works()
	{
		var pre = Localization.Get("pre-amazon - germany");
		pre.Name.ShouldBe("pre-amazon - germany");
		pre.WithUsername.ShouldBeTrue();
	}
}
