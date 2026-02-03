namespace Authoriz.IdentityTests;

[TestClass]
public class ctor_locale
{
	[TestMethod]
	public void null_params_throw()
		=> Assert.Throws<ArgumentNullException>(() => new Identity(null!));

	[TestMethod]
	public void invalid()
	{
		var us = new Identity(Localization.Get("us"));
		us.IsValid.ShouldBeFalse();
		us.Locale.Name.ShouldBe("us");
	}
}

[TestClass]
public class ctor_locale_accessToken_cookies
{
	[TestMethod]
	public void null_params_throw()
	{
		Assert.Throws<ArgumentNullException>(() => new Identity(
			null!,
			OAuth2.Empty,
			new List<KeyValuePair<string, string?>>()));
		Assert.Throws<ArgumentNullException>(() => new Identity(
			Locale.Empty,
			null!,
			new List<KeyValuePair<string, string?>>()));
	}

	[TestMethod]
	public void loads_cookies()
	{
		var idMgr = new Identity(Locale.Empty, OAuth2.Empty, new List<KeyValuePair<string, string?>> { new KeyValuePair<string, string?>("name1", "value1") });

		idMgr.Cookies.Count().ShouldBe(1);
		idMgr.Cookies.Single().Key.ShouldBe("name1");
		idMgr.Cookies.Single().Value.ShouldBe("value1");
	}
}

[TestClass]
public class Empty
{
	[TestMethod]
	public void ensure_valid()
	{
		// most important part is to get past this line w/o exceptions
		var i = Identity.Empty;
		i.IsValid.ShouldBeFalse();
	}
}
