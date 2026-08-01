namespace Authoriz.RefreshTokenTests;

[TestClass]
public class ValidateInput
{
	[TestMethod]
	public void null_throws()
		=> Assert.Throws<ArgumentNullException>(() => new RefreshToken(null!));

	[TestMethod]
	public void blank_throws()
	{
		Assert.Throws<ArgumentException>(() => new RefreshToken(""));
		Assert.Throws<ArgumentException>(() => new RefreshToken("   "));
	}

	[TestMethod]
	public void bad_beginning()
	{
		Assert.Throws<ArgumentException>(() => new RefreshToken("foo"));
	}

	[TestMethod]
	public void valid_strings()
	{
		var justBeginning = "Atnr|";
		new RefreshToken(justBeginning)
			.Value.ShouldBe(justBeginning);

		var full = "Atnr|foo";
		new RefreshToken(full)
			.Value.ShouldBe(full);
	}
}

[TestClass]
public class ToString
{
	[TestMethod]
	public void redacts_value()
	{
		var full = "Atnr|foo";
		var text = new RefreshToken(full).ToString();
		text.ShouldBe($"RefreshToken [REDACTED length={full.Length}]");
		text.ShouldNotContain(full);
	}
}
