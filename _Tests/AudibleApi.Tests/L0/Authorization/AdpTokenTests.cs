namespace Authoriz.AdpTokenTests
{
    [TestClass]
	public class ValidateInput
	{
		[TestMethod]
		public void null_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new AdpToken(null));

		[TestMethod]
		public void blank_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken(""));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("   "));
		}

		[TestMethod]
		public void bad_format()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("no braces"));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{no end brace"));
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("no begin end}"));
		}

		[TestMethod]
		public void missing_entry()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}"));
		}

		[TestMethod]
		public void extra_entry()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:}{foo:bar}"));
		}

		[TestMethod]
		public void bad_name()
		{
			Assert.ThrowsException<ArgumentException>(() => new AdpToken("{enc:}{key:}{iv:}{name:foo}{serial:}"));
		}

		[TestMethod]
		public void valid_strings()
		{
			var min = "{enc:}{key:}{iv:}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:}";
			new AdpToken(min)
				.Value.ShouldBe(min);

			var better = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
			new AdpToken(better)
				.Value.ShouldBe(better);
		}

		[TestMethod]
		public void test_parser()
		{
			var enc = "asdfghjkl==";
			var key = "123456789==";
			var iv = "1a2s3d4==";
			var name = "QURQVG9rZW5FbmNyeXB0aW9uS2V5";
			var serial = "Mg==";
			var str
				= $"{{enc:{enc}}}"
				+ $"{{key:{key}}}"
				+ $"{{iv:{iv}}}"
				+ $"{{name:{name}}}"
				+ $"{{serial:{serial}}}";

			var dic = AdpToken.adp_parser.Parse(str);

			dic.Count.ShouldBe(5);
			dic.ContainsKey("enc").ShouldBeTrue();
			dic.ContainsKey("key").ShouldBeTrue();
			dic.ContainsKey("iv").ShouldBeTrue();
			dic.ContainsKey("name").ShouldBeTrue();
			dic.ContainsKey("serial").ShouldBeTrue();
			dic["enc"].ShouldBe(enc);
			dic["key"].ShouldBe(key);
			dic["iv"].ShouldBe(iv);
			dic["name"].ShouldBe(name);
			dic["serial"].ShouldBe(serial);
		}
	}
}
