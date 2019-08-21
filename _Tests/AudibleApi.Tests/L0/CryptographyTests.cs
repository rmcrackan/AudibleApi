using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using BaseLib;
using FluentAssertions;
using Jint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static TestAudibleApiCommon.ComputedTestValues;

namespace CryptographyTests
{
    internal static class JintEngineExtensions
    {
        public static string GetCompletionString(this Engine engine)
            => engine.GetCompletionValue()?.ToObject()?.ToString();
    }

    [TestClass]
    public class javascript
    {
        public static string plaintext => CredentialsPageMetadataPlaintext;
		string update_val { get; } = "1292237513";
        string hex_hash { get; } = "4D05FAC9";
        string str_to_parse => hex_hash + "#" + plaintext;
        string parsed_length { get; } = "5048";
        static string evaled => CredentialsPageMetadataEncrypted.Substring("ECdITeCs:".Length);
		public static string encrypted => CredentialsPageMetadataEncrypted;

        Engine engine;
        List<object> list { get; } = new List<object>();

        void log(object o)
        {
            Console.WriteLine(o);
            list.Add(o);
        }

        [TestInitialize]
        public void TestInit()
        {
            engine = new Engine()
                .SetValue("log", new Action<object>(log))
                .Execute(Cryptography.Javascript);
        }

        [TestMethod]
        public void js_log()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);
                ")
                .GetCompletionString();
            list.Count.Should().Be(1);
            list.Single().Should().Be("metadata:" + plaintext);
            value.Should().BeNull();
        }

        [TestMethod]
        public void js_update()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    // return
                    update_val
                ")
                .GetCompletionString();
            list.Count.Should().Be(2);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            value.Should().Be(update_val);
        }

        [TestMethod]
        public void js_format()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    var hex_hash = format(update_val);
                    log('hex_hash:' + hex_hash);

                    // return
                    hex_hash
                ")
                .GetCompletionString();
            list.Count.Should().Be(3);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            list[2].Should().Be("hex_hash:" + hex_hash);
            value.Should().Be(hex_hash);
        }

        [TestMethod]
        public void js_str_to_parse()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    var hex_hash = format(update_val);
                    log('hex_hash:' + hex_hash);

                    var str_to_parse = hex_hash + '#' + metadata;
                    log('str_to_parse:' + str_to_parse);

                    // return
                    str_to_parse
                ")
                .GetCompletionString();
            list.Count.Should().Be(4);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            list[2].Should().Be("hex_hash:" + hex_hash);
            list[3].Should().Be("str_to_parse:" + str_to_parse);
            value.Should().Be(str_to_parse);
        }

        [TestMethod]
        public void js_parse()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    var hex_hash = format(update_val);
                    log('hex_hash:' + hex_hash);

                    var str_to_parse = hex_hash + '#' + metadata;
                    log('str_to_parse:' + str_to_parse);

                    var parsed = parse(str_to_parse);
                    log('parsed.length:' + parsed.length);

                    // return
                    parsed.length
                ")
                .GetCompletionString();
            list.Count.Should().Be(5);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            list[2].Should().Be("hex_hash:" + hex_hash);
            list[3].Should().Be("str_to_parse:" + str_to_parse);
            list[4].Should().Be("parsed.length:" + parsed_length);
            value.Should().Be(parsed_length);
        }

        [TestMethod]
        public void js_evaluate()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    var hex_hash = format(update_val);
                    log('hex_hash:' + hex_hash);

                    var str_to_parse = hex_hash + '#' + metadata;
                    log('str_to_parse:' + str_to_parse);

                    var parsed = parse(str_to_parse);
                    log('parsed.length:' + parsed.length);

                    var evaled = evaluate(parsed);
                    log('evaled:' + evaled);

                    // return
                    evaled
                ")
                .GetCompletionString();
            list.Count.Should().Be(6);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            list[2].Should().Be("hex_hash:" + hex_hash);
            list[3].Should().Be("str_to_parse:" + str_to_parse);
            list[4].Should().Be("parsed.length:" + parsed_length);
            list[5].Should().Be("evaled:" + evaled);
            value.Should().Be(evaled);
        }

        [TestMethod]
        public void js_encrypted_final()
        {
            var value = engine
                .Execute($@"
                    var metadata = '{plaintext}';
                    log('metadata:' + metadata);

                    var update_val = update(metadata);
                    log('update_val:' + update_val);

                    var hex_hash = format(update_val);
                    log('hex_hash:' + hex_hash);

                    var str_to_parse = hex_hash + '#' + metadata;
                    log('str_to_parse:' + str_to_parse);

                    var parsed = parse(str_to_parse);
                    log('parsed.length:' + parsed.length);

                    var evaled = evaluate(parsed);
                    log('evaled:' + evaled);

                    var final = 'ECdITeCs:' + evaled;
                    log('final:' + final);

                    // return
                    final
                ")
                .GetCompletionString();
            list.Count.Should().Be(7);
            list[0].Should().Be("metadata:" + plaintext);
            list[1].Should().Be("update_val:" + update_val);
            list[2].Should().Be("hex_hash:" + hex_hash);
            list[3].Should().Be("str_to_parse:" + str_to_parse);
            list[4].Should().Be("parsed.length:" + parsed_length);
            list[5].Should().Be("evaled:" + evaled);
            list[6].Should().Be("final:" + encrypted);
            value.Should().Be(encrypted);
        }
    }

    [TestClass]
    public class EncryptMetadata
    {
        [TestMethod]
        public void null_param_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => Cryptography.EncryptMetadata(null));

        [TestMethod]
        public void encrypt()
            => Cryptography
                .EncryptMetadata(javascript.plaintext)
                .Should().Be(javascript.encrypted);
    }

    [TestClass]
    public class SignRequest
    {
        [TestMethod]
        public void null_params_throw()
        {
            var message = new HttpRequestMessage();
            var adpToken = new AdpToken(AdpTokenValue);
			var privateKey = new PrivateKey(PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING);

            Assert.ThrowsException<ArgumentNullException>(() => Cryptography.SignRequest(null, date, adpToken, privateKey));
            Assert.ThrowsException<ArgumentNullException>(() => Cryptography.SignRequest(message, date, null, privateKey));
            Assert.ThrowsException<ArgumentNullException>(() => Cryptography.SignRequest(message, date, adpToken, null));
        }

        AdpToken adpToken { get; } = new AdpToken(AdpTokenValue);
        PrivateKey privateKey { get; } = new PrivateKey(PrivateKeyValue);
        DateTime date { get; } = new DateTime(2016, 2, 15, 10, 20, 30, DateTimeKind.Utc);

        [TestMethod]
        public void headers_are_populated()
        {
            // no body
            var request = new HttpRequestMessage(HttpMethod.Get, "/1.0/library?purchaseAfterDate=01/01/1970");

            Cryptography.SignRequest(request, date, adpToken, privateKey);

            var headers = request.Headers.ToList();
            headers.Single(kvp => kvp.Key == "x-adp-token").Value.Single()
                .Should().Be(AdpTokenValue);
            headers.Single(kvp => kvp.Key == "x-adp-alg").Value.Single()
                .Should().Be("SHA256withRSA:1.0");

            headers.Single(kvp => kvp.Key == "x-adp-signature").Value.Single()
                .Should().Be(Encryption_Get);
        }

        [TestMethod]
        public void sign_GET()
		{
			var signature = CreateGetSignature();
			signature.Should().Be(Encryption_Get);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public string CreateGetSignature()
		{
			// no body
			var request = new HttpRequestMessage(HttpMethod.Get, "/1.0/library?purchaseAfterDate=01/01/1970");

			var signature = Cryptography.CalculateSignature(request, date, adpToken, privateKey);
			return signature;
		}

		[TestMethod]
        public void sign_POST()
		{
			var signature = CreatePostSignature();
			signature.Should().Be(Encryption_Post);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public string CreatePostSignature()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "/1.0/wishlist");
			request.AddContent(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("asin", "B002V02KPU") });

			var signature = Cryptography.CalculateSignature(request, date, adpToken, privateKey);
			return signature;
		}

		[TestMethod]
        public void sign_DELETE()
		{
			var signature = CreateDeleteSignature();
			signature.Should().Be(Encryption_Delete);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public string CreateDeleteSignature()
		{
			// no body
			var request = new HttpRequestMessage(HttpMethod.Delete, "/1.0/wishlist/B002V02KPU");

			var signature = Cryptography.CalculateSignature(request, date, adpToken, privateKey);
			return signature;
		}
	}
}
