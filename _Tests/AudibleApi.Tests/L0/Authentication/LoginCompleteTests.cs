namespace Authentic.LoginCompleteTests;

[TestClass]
public class ctor
{
	[TestMethod]
	public void null_identity_throws()
		=> Assert.Throws<ArgumentNullException>(() => new LoginComplete(AuthenticateShared.GetAuthenticate(), "x", null!));
}
