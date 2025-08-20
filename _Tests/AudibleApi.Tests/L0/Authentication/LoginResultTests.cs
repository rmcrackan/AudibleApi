namespace Authentic.LoginResultTests
{
    internal class ValidateLoginResult : LoginResult
    {
        public ValidateLoginResult(Authenticate authenticate, string responseBody) : base(authenticate, responseBody) { }
    }

    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_authenticate_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(null, "foo"));

        [TestMethod]
        public void null_responseBody_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new ValidateLoginResult(AuthenticateShared.GetAuthenticate(), null));

        [TestMethod]
        public void inputs_are_saved()
        {
            var body
                = "<input name='a' value='b' />"
                + "<input name='y' value='z' />";
            var result = new ValidateLoginResult(AuthenticateShared.GetAuthenticate(), body);
            var inputs = result.GetInputsReadOnly();
            inputs.Count.ShouldBe(2);
            inputs["a"].ShouldBe("b");
            inputs["y"].ShouldBe("z");
        }
    }
}
