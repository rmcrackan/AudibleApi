﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;
using static AuthorizationShared.Shared.AccessTokenTemporality;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.IdentityPersistentTests
{
    public class IdentityPersistentTestBase
    {
        protected string TestFile;

        protected void WriteToTestFile(string contents)
            => File.WriteAllText(TestFile, contents);

        // create file with valid contents
        protected void CreateValidIdentityFile()
            => WriteToTestFile(GetIdentityJson(Future));
        protected void CreateValidNestedIdentityFile()
            => WriteToTestFile(GetNestedIdentityJson(Future));

        [TestInitialize]
        public void TestInit()
            => TestFile = Guid.NewGuid() + ".txt";

        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(TestFile))
                File.Delete(TestFile);
        }
    }

    [TestClass]
    public class ctor_Identity_path : IdentityPersistentTestBase
    {
		[TestMethod]
        public void null_path_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(new Mock<IIdentity>().Object, null));

        [TestMethod]
        public void blank_path_throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent(new Mock<IIdentity>().Object, ""));
            Assert.ThrowsException<ArgumentException>(() => new IdentityPersistent(new Mock<IIdentity>().Object, "   "));
        }

        [TestMethod]
        public void null_IIdentity_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(null, "foo"));

		[TestMethod]
		public void invalid_state_identity_saves()
		{
			var id = new Identity(
                Locale.Empty,
                AccessToken.Empty,
                new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("k", "val") });
			id.IsValid.Should().BeFalse();

			new IdentityPersistent(id, TestFile);
			var contents = File.ReadAllText(TestFile);

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

        [TestMethod]
        [DataRow(null, null)]
        [DataRow("", null)]
        [DataRow("   ", null)]
        public void set_jsonpath_null(string jsonPath, string expected)
        {
            var idMgr = GetIdentity(Future);
            new IdentityPersistent(idMgr, TestFile, jsonPath)
                .JsonPath.Should().Be(expected);
        }

        [TestMethod]
        public void set_jsonpath_non_null_invalid_file()
        {
            var idMgr = GetIdentity(Future);
            Assert.ThrowsException<FileNotFoundException>(() => new IdentityPersistent(idMgr, $@"C:\{Guid.NewGuid()}.txt", "foo"));
        }

        [TestMethod]
        public void set_jsonpath_non_null_non_matching_file()
        {
            // create identity with no nesting. does not match JsonPathMatch
            CreateValidIdentityFile();

            var idMgr = GetIdentity(Future);
            Assert.ThrowsException<JsonSerializationException>(() => new IdentityPersistent(idMgr, TestFile, JsonPathMatch));
        }

        [TestMethod]
        public void set_jsonpath_non_null()
        {
            var jsonPath = JsonPathMatch;

            CreateValidNestedIdentityFile();
            var idMgr = GetIdentity(Future);
            new IdentityPersistent(idMgr, TestFile, jsonPath)
                .JsonPath.Should().Be(jsonPath);
        }

        [TestMethod]
        public void set_jsonpath_non_null_trim()
        {
            var jsonPath = JsonPathMatch;

            CreateValidNestedIdentityFile();
            var idMgr = GetIdentity(Future);
            new IdentityPersistent(idMgr, TestFile, $"   {jsonPath}   ")
                .JsonPath.Should().Be(jsonPath);
        }
    }

    [TestClass]
    public class ctor_path : IdentityPersistentTestBase
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
            => Assert.ThrowsException<FileNotFoundException>(() => new IdentityPersistent(TestFile));

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void set_jsonpath_null(string jsonPath)
        {
            CreateValidIdentityFile();
            new IdentityPersistent(TestFile, jsonPath)
                .JsonPath.Should().BeNull();
        }

        [TestMethod]
        public void set_jsonpath_non_null()
        {
            var jsonPath = JsonPathMatch;

            CreateValidNestedIdentityFile();
            new IdentityPersistent(TestFile, jsonPath)
                .JsonPath.Should().Be(jsonPath);
        }

        [TestMethod]
        public void set_jsonpath_non_null_trim()
        {
            var jsonPath = JsonPathMatch;

            CreateValidNestedIdentityFile();
            new IdentityPersistent(TestFile, $"   {jsonPath}   ")
                .JsonPath.Should().Be(jsonPath);
        }
    }

    [TestClass]
    public class loadFromPath : IdentityPersistentTestBase
    {
        [TestMethod]
        public void file_is_valid()
		{
			CreateValidIdentityFile();

			var idMgrPersist = new IdentityPersistent(TestFile);

			is_valid(idMgrPersist);
		}

		[TestMethod]
        public void file_is_blank()
        {
            WriteToTestFile("");
            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void file_is_wrong_format()
        {
            WriteToTestFile("foo");
            Assert.ThrowsException<JsonReaderException>(() => new IdentityPersistent(TestFile));
        }

		[TestMethod]
        public void file_missing_accessToken()
		{
			var contents = $@"
{{
  ""PrivateKey"": {{
    ""Value"": ""{PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING}""
  }},
  ""AdpToken"": {{
    ""Value"": ""{AdpTokenValue}""
  }},
  ""RefreshToken"": {{
    ""Value"": ""{RefreshToken.REQUIRED_BEGINNING}""
  }},
  ""Cookies"": []
}}
".Trim();
            WriteToTestFile(contents);
            Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(TestFile));
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
            WriteToTestFile(contents);
            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void valid_matching_jsonPath()
        {
            CreateValidNestedIdentityFile();

            var idMgrPersist = new IdentityPersistent(TestFile, JsonPathMatch);

            is_valid(idMgrPersist);
        }

        private static void is_valid(IdentityPersistent idMgrPersist)
        {
            idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
            idMgrPersist.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
            idMgrPersist.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
            idMgrPersist.AdpToken.Value.Should().Be(AdpTokenValue);
            idMgrPersist.RefreshToken.Value.Should().Be(RefreshTokenValue);
        }

        [TestMethod]
        public void valid_nonmatching_jsonPath()
        {
            CreateValidNestedIdentityFile();

            Assert.ThrowsException<JsonSerializationException>(() => new IdentityPersistent(TestFile, JsonPathNonMatch));
        }

        [TestMethod]
        public void invalid_jsonPath()
		{
            var jsonPath = "$[";

            CreateValidNestedIdentityFile();

            Assert.ThrowsException<JsonException>(() => new IdentityPersistent(TestFile, jsonPath));
		}
    }

    [TestClass]
    public class save : IdentityPersistentTestBase
    {
        [TestMethod]
        public void invalid_path()
        {
            var invalidPath = @"Q:\";
            var currfile = Path.Combine(invalidPath, TestFile);

            Assert.ThrowsException<DirectoryNotFoundException>(() => new IdentityPersistent(currfile));
        }

        [TestMethod]
        public void file_does_not_exist()
            => Assert.ThrowsException<FileNotFoundException>(() => new IdentityPersistent(TestFile));

        [TestMethod]
        public void file_is_empty()
        {
            WriteToTestFile("");
            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void file_is_whitespace()
        {
            WriteToTestFile("   ");
            Assert.ThrowsException<FormatException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void file_is_wrong_format()
        {
            WriteToTestFile("foo");
            Assert.ThrowsException<JsonReaderException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void file_has_valid_values()
        {
            CreateValidIdentityFile();

            // new object contains valid contents
            var idMgrPersist = new IdentityPersistent(TestFile);
            idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
        }

        [TestMethod]
        public void extra_fields_are_ok()
        {
            var contentsWithExtraField = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": ""{AccessTokenValue}"",
    ""Expires"": ""{GetAccessTokenExpires(Future)}""
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
            WriteToTestFile(contentsWithExtraField);

            // new object contains valid contents
            var idMgrPersist = new IdentityPersistent(TestFile);
            idMgrPersist.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
        }

        [TestMethod]
        public void file_with_missing_fields_loads_as_invalid()
        {
            var contentsNoRefreshToken = $@"
{{
  ""ExistingAccessToken"": {{
    ""TokenValue"": ""{AccessTokenValue}"",
    ""Expires"": ""{GetAccessTokenExpires(Future)}""
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
            WriteToTestFile(contentsNoRefreshToken);

            var idPersist = new IdentityPersistent(TestFile);
			idPersist.IsValid.Should().BeFalse();
			idPersist.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idPersist.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
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
            WriteToTestFile(json);
            Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(TestFile));
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
            WriteToTestFile(json);
            Assert.ThrowsException<ArgumentNullException>(() => new IdentityPersistent(TestFile));
        }

        [TestMethod]
        public void save_to_new_file()
        {
            var idMgr = GetIdentity(Future);

            new IdentityPersistent(idMgr, TestFile);

            var contents = File.ReadAllText(TestFile);
            var idMgrOut = Identity.FromJson(contents);
            idMgrOut.AdpToken.Value.Should().Be(idMgr.AdpToken.Value);
            idMgrOut.ExistingAccessToken.Expires.Should().Be(idMgr.ExistingAccessToken.Expires);
        }

        [TestMethod]
        public void overwrite_existing_file()
        {
            var idMgr = GetIdentity(Future);

            // start with invalid file
            WriteToTestFile("line 1\r\n2\r\n\r\n4");

            // overwrites with valid identity
            new IdentityPersistent(idMgr, TestFile);

            var contents = File.ReadAllText(TestFile);
            var idMgrOut = Identity.FromJson(contents);
            idMgrOut.AdpToken.Value.Should().Be(idMgr.AdpToken.Value);
            idMgrOut.ExistingAccessToken.Expires.Should().Be(idMgr.ExistingAccessToken.Expires);
        }

        [TestMethod]
        public void jsonpath_ctor_identity_param_overwrites_file()
        {
            var jsonPath = JsonPathMatch;

            CreateValidNestedIdentityFile();

            // verify file contents
            var contents = File.ReadAllText(TestFile);
            JObject
                .Parse(contents)
                .SelectToken(jsonPath)["ExistingAccessToken"]
                .Value<string>("TokenValue")
                .Should().Be(AccessTokenValue);

            // alter identity
            var newAtValue = AccessTokenValue + "_NEW";
            var newDt = DateTime.Now.AddDays(1);
            var newAt = new AccessToken(newAtValue, newDt);

            var idMgr = GetIdentity(Future);
            idMgr.Update(newAt);

            // this step writes altered identity to file
            new IdentityPersistent(idMgr, TestFile, jsonPath)
                .JsonPath.Should().Be(jsonPath);

            // verify new file contents
            var newContents = File.ReadAllText(TestFile);
            JObject
                .Parse(newContents)
                .SelectToken(jsonPath)["ExistingAccessToken"]
                .Value<string>("TokenValue")
                .Should().Be(newAtValue);
        }
    }
}
