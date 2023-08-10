namespace Authoriz.RefreshTokenTests
{
    [TestClass]
	public class ValidateInput
	{
		[TestMethod]
		public void null_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new RefreshToken(null));

		[TestMethod]
		public void blank_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken(""));
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken("   "));
		}

		[TestMethod]
		public void bad_beginning()
		{
			Assert.ThrowsException<ArgumentException>(() => new RefreshToken("foo"));
		}

		[TestMethod]
		public void valid_strings()
		{
			var justBeginning = "Atnr|";
			new RefreshToken(justBeginning)
				.Value.Should().Be(justBeginning);

			var full = "Atnr|foo";
			new RefreshToken(full)
				.Value.Should().Be(full);
		}
	}
}
