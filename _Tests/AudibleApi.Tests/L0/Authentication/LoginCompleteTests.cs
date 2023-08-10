namespace Authentic.LoginCompleteTests
{
    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_identity_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new LoginComplete(AuthenticateShared.GetAuthenticate(), "x", null));
    }
}
