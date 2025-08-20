namespace Authoriz.AccessTokenTests
{
    [TestClass]
    public class ctor
	{
		[TestMethod]
		public void null_value_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new AccessToken(null, DateTime.MinValue));

		[TestMethod]
        public void blank_string_throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new AccessToken("", DateTime.MinValue));
            Assert.ThrowsException<ArgumentException>(() => new AccessToken("   ", DateTime.MinValue));
        }

		[TestMethod]
		public void bad_beginning()
		{
			Assert.ThrowsException<ArgumentException>(() => new AccessToken("foo", DateTime.MinValue));
		}

		[TestMethod]
		public void valid_strings()
		{
			var justBeginning = "Atna|";
			new AccessToken(justBeginning, DateTime.MinValue)
				.TokenValue.ShouldBe(justBeginning);

			var full = "Atna|foo";
			new AccessToken(full, DateTime.MinValue)
				.TokenValue.ShouldBe(full);
		}
	}

	[TestClass]
	public class Empty
	{
		[TestMethod]
		public void ensure_valid()
		{
			// most important part is to get past this line w/o exceptions
			var e = AccessToken.Empty;

			e.TokenValue.Length.ShouldBeGreaterThan(2);
			e.TokenValue.Length.ShouldBeLessThan(10);

			e.Expires.ShouldBe(DateTime.MinValue);
		}
	}

    [TestClass]
    public class Invalidate
    {
        [TestMethod]
        public void invalidate_token()
        {
            var token = new AccessToken("Atna|foo", DateTime.MaxValue);
            token.Invalidate();
            token.Expires.ShouldBe(DateTime.MinValue);
        }
    }

	[TestClass]
	public class ToString
	{
		[TestMethod]
		public void print()
		{
			var dateTime = DateTime.MaxValue;
			new AccessToken("Atna|foo", dateTime)
				.ToString()
				.ShouldBe(
				$"AccessToken. Value=Atna|foo. Expires={dateTime}"
				);
		}
	}
}
