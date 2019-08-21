using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TestAudibleApiCommon;
using TestCommon;
using static Authoriz.IdentityPersistentTests.Shared;
using static AuthorizationShared.Shared;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.IdentityPersistentTests
{
    public static class Shared
    {
        public static string getRandomFilePath()
            => Guid.NewGuid().ToString() + ".txt";
    }

    [TestClass]
    public class ctor_path_Identity
	{
		string currfile;

		[TestCleanup]
		public void TestCleanup()
		{
			if (File.Exists(currfile))
				File.Delete(currfile);
		}

		[TestMethod]
        public void null_path_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(null, new Mock<IIdentity>().Object));

        [TestMethod]
        public void blank_path_throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent("", new Mock<IIdentity>().Object));
            Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent("   ", new Mock<IIdentity>().Object));
        }

        [TestMethod]
        public void null_IIdentity_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent("foo", null));

		string at { get; } = AccessToken.REQUIRED_BEGINNING;

		[TestMethod]
		public void invalid_state_identity_saves()
		{
			var id = new Identity(new AccessToken(at, DateTime.MaxValue), new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("k", "val") });
			id.IsValid.Should().BeFalse();

			currfile = getRandomFilePath();

			new IdentityPersistent(currfile, id);
			var contents = File.ReadAllText(currfile);

			var idOut = Identity.FromJson(contents);
			idOut.IsValid.Should().BeFalse();
			idOut.ExistingAccessToken.TokenValue.Should().Be(id.ExistingAccessToken.TokenValue);
			idOut.ExistingAccessToken.Expires.Should().Be(id.ExistingAccessToken.Expires);
			var cookies = idOut.Cookies.ToKeyValuePair();
			cookies.Count().Should().Be(1);
			var cookie = cookies.ToList()[0];
			cookie.Key.Should().Be("k");
			cookie.Value.Should().Be("val");
		}
    }

    [TestClass]
    public class ctor_path_systemDateTime
	{
		[TestMethod]
		public void null_path_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(null));

		[TestMethod]
		public void blank_path_throws()
		{
			Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent(""));
			Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent("   "));
		}

        [TestMethod]
        public void file_does_not_exist()
            => Assert.ThrowsException<FileNotFoundException>(() => new IdentityPersistent(getRandomFilePath()));
    }

    [TestClass]
    public class loadFromPath
    {
        [TestMethod]
        public void file_is_valid()
        {
            var path = getRandomFilePath();
            try
            {
                File.WriteAllText(path, IdentityJson_Future);

				var idMgrPersist = new IdentityPersistent(path);

                idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
                idMgrPersist.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
                idMgrPersist.ExistingAccessToken.Expires.Should().Be(AccessTokenExpires_Future_Parsed);
                idMgrPersist.AdpToken.Value.Should().Be(AdpTokenValue);
                idMgrPersist.RefreshToken.Value.Should().Be(RefreshTokenValue);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void file_is_blank()
        {
            var path = getRandomFilePath();
            try
            {
                File.WriteAllText(path, "");

                Assert.ThrowsException<FormatException>(() => new IdentityPersistent(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void file_is_wrong_format()
        {
            var path = getRandomFilePath();
            try
            {
                File.WriteAllText(path, "foo");

                Assert.ThrowsException<Newtonsoft.Json.JsonReaderException>(() => new IdentityPersistent(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

		string pk { get; } = PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING;
		string adp { get; } = AdpTokenValue;
		string rt { get; } = RefreshToken.REQUIRED_BEGINNING;

		[TestMethod]
        public void file_missing_accessToken()
		{
			var contents = $@"
{{
  ""PrivateKey"": {{
    ""Value"": ""{pk}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{adp}""
  }},
  ""RefreshToken"": {{
    ""Value"": ""{rt}""
  }},
  ""Cookies"": []
}}
".Trim();

			var path = getRandomFilePath();
			try
			{
				File.WriteAllText(path, contents);
				Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(path));
			}
			finally
			{
				File.Delete(path);
			}
		}

		[TestMethod]
        [DataRow("pk", "at", "adp", "rt")]
        public void file_is_incomplete_no_expires(string privateKey, string accessToken, string adpToken, string refreshToken)
        {
            string expires = null;
            var contents = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": ""{accessToken}"",
    ""Expires"": ""{expires}""
  }},
  ""PrivateKey"": {{
    ""Value"": ""{privateKey}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{adpToken}""
  }},
  ""RefreshToken"": {{
    ""Value"": ""{refreshToken}""
  }},
  ""Cookies"": []
}}
".Trim();

            var path = getRandomFilePath();
            try
            {
                File.WriteAllText(path, contents);
                Assert.ThrowsException<FormatException>(() => new IdentityPersistent(path));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }

    [TestClass]
    public class save
    {
        string currfile;

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(currfile))
                File.Delete(currfile);
        }

        [TestMethod]
        public void invalid_path()
        {
            var invalidPath = @"Q:\";
            currfile = Path.Combine(invalidPath, getRandomFilePath());

            Assert.ThrowsException<DirectoryNotFoundException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_does_not_exist()
        {
            currfile = getRandomFilePath();

            Assert.ThrowsException<FileNotFoundException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_is_empty()
        {
            currfile = getRandomFilePath();
            var contents = "";
            File.WriteAllText(currfile, contents);

            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_is_whitespace()
        {
            currfile = getRandomFilePath();
            var contents = "   ";
            File.WriteAllText(currfile, contents);

            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_is_wrong_format()
        {
            currfile = getRandomFilePath();
            var contents = "foo";
            File.WriteAllText(currfile, contents);

            Assert.ThrowsException<Newtonsoft.Json.JsonReaderException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_has_valid_values()
        {
            // create file with valid contents
            currfile = getRandomFilePath();
            File.WriteAllText(currfile, IdentityJson_Future);

            // new object contains valid contents
            var idMgrPersist = new IdentityPersistent(currfile);
            idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
        }

        [TestMethod]
        public void extra_fields_are_ok()
        {
            var contentsWithExtraField = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": ""{AccessTokenValue}"",
    ""Expires"": ""{AccessTokenExpires_Future}""
  }},
  ""EXTRAFIELD"": ""foo"",
  ""PrivateKey"": {{
    ""Value"": ""{PrivateKeyValueNewLines}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{AdpTokenValue}""
  }},
  ""RefreshToken"": {{
    ""Value"": ""{RefreshTokenValue}""
  }},
  ""Cookies"": []
}}
".Trim();

            // create file with valid contents
            currfile = getRandomFilePath();
            File.WriteAllText(currfile, contentsWithExtraField);

            // new object contains valid contents
            var idMgrPersist = new IdentityPersistent(currfile);
            idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
        }

        [TestMethod]
        public void file_with_missing_fields_loads_as_invalid()
        {
            var contentsNoRefreshToken = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": ""{AccessTokenValue}"",
    ""Expires"": ""{AccessTokenExpires_Future}""
  }},
  ""PrivateKey"": {{
    ""Value"": ""{PrivateKeyValueNewLines}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{AdpTokenValue}""
  }},
  ""Cookies"": []
}}
".Trim();

            // create file missing a field
            currfile = getRandomFilePath();
            File.WriteAllText(currfile, contentsNoRefreshToken);
			var idPersist = new IdentityPersistent(currfile);
			idPersist.IsValid.Should().BeFalse();
			idPersist.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idPersist.ExistingAccessToken.Expires.Should().Be(DateTime.Parse(AccessTokenExpires_Future));
			idPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idPersist.AdpToken.Value.Should().Be(AdpTokenValue);
		}

		[TestMethod]
		public void file_with_missing_access_token()
		{
			var json = $@"
{{
  ""PrivateKey"": {{
    ""Value"": ""{PrivateKeyValueNewLines}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{AdpTokenValue}""
  }},
  ""Cookies"": []
}}
".Trim();
			currfile = getRandomFilePath();
			File.WriteAllText(currfile, json);

			Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(currfile));
		}

		[TestMethod]
        public void file_is_valid_format_with_empty_values()
        {
            var json = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": null,
    ""Expires"": ""{DateTime.MinValue}""
  }},
  ""PrivateKey"": {{
    ""Value"": null
  }},
  ""AdpToken"": {{
    ""Value"": null
  }},
  ""RefreshToken"": {{
    ""Value"": null
  }},
  ""Cookies"": []
}}
".Trim();
            currfile = getRandomFilePath();
            File.WriteAllText(currfile, json);

            Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void save_to_new_file()
        {
            var idMgr = GetIdentity_Future();
            currfile = getRandomFilePath();

            new IdentityPersistent(currfile, idMgr);
            var contents = File.ReadAllText(currfile);

            var idMgrOut = Identity.FromJson(contents);
            idMgrOut.AdpToken.Value.Should().Be(idMgr.AdpToken.Value);
            idMgrOut.ExistingAccessToken.Expires.Should().Be(idMgr.ExistingAccessToken.Expires);
        }

        [TestMethod]
        public void overwrite_existing_file()
        {
            var idMgr = GetIdentity_Future();
            currfile = getRandomFilePath();

            File.WriteAllText(currfile, "line 1\r\n2\r\n\r\n4");

            new IdentityPersistent(currfile, idMgr);
            var contents = File.ReadAllText(currfile);

            var idMgrOut = Identity.FromJson(contents);
            idMgrOut.AdpToken.Value.Should().Be(idMgr.AdpToken.Value);
            idMgrOut.ExistingAccessToken.Expires.Should().Be(idMgr.ExistingAccessToken.Expires);
        }
    }
}
