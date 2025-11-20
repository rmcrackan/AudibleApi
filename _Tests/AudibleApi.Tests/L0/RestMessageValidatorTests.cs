using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace RestMessageValidatorTests
{
    [TestClass]
    public class ThrowStrongExceptionsIfInvalid
    {
        public static void test(JObject jObj)
            => test(jObj?.ToString());
        public static void test(string str)
            => RestMessageValidator.ThrowStrongExceptionsIfInvalid(str, new Uri("http://test.com"));

        [TestMethod]
        public void no_error_null() => test((string)null);

        [TestMethod]
        public void no_error_string() => test("hi");

        [TestMethod]
        public void no_error_json() => test(new JObject { { "a", 1 } });

        [TestMethod]
        public void throws_NotAuthenticatedException()
            => Assert.Throws<NotAuthenticatedException>(() => test(new JObject
            {
                { "message", "Message could not be authenticated" }
            }));

        [TestMethod]
        public void throws_InvalidResponseException()
            => Assert.Throws<InvalidResponseException>(() => test(new JObject
            {
                { "message", "Invalid response group" }
            }));

        [TestMethod]
        public void throws_InvalidValueException()
            => Assert.Throws<InvalidValueException>(() => test(new JObject
            {
                { "error", "InvalidValue" }
            }));

        [TestMethod]
        public void throws_ApiErrorException()
            => Assert.Throws<ApiErrorException>(() => test(new JObject
            {
                { "error", "Unknown Error Test" }
            }));

        [TestMethod]
        public void throws_ValidationErrorException()
        {
            string[] messages = {
                "validation error detected",
                "validation errors detected",
                "1 validation error detected: Value null at 'asin' failed to satisfy constraint: Member must not be null",
                @"1 validation error detected: Value '-1' at 'page' failed to satisfy constraint: Member must satisfy regular expression pattern: ^\\d+$"
            };
            foreach (var msg in messages)
                Assert.Throws<ValidationErrorException>(
                    () => test(new JObject { { "message", msg } })
                );
        }

        [TestMethod]
        public void throws_no_response_groups()
        {
            var msg = new JObject
            {
                { "error_code", "000307" },
                { "message", "No response groups populated." }
            };
            var ex = Assert.Throws<ApiErrorException>(() => test(msg));
        }

        [TestMethod]
        public void internal_server_error()
            => Assert.Throws<ApiErrorException>(
       () => test(new JObject { { "message", "Whoops! Looks like something went wrong." } }));
    }
}
