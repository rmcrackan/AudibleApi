//// uncomment to run these tests
//#define RUN_LIVE_DOWNLOADS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SystemNetHttpExtensionsTests
{
    [TestClass]
    public class AddContent
    {
        [TestMethod]
        public void string_test()
        {
            var request = getEmptyMessage();

            var input = "my string";
            var content = new StringContent(input);

            request.AddContent(content);

            Assert.AreEqual(request.Content.Headers.ContentType.CharSet, "utf-8");
            Assert.AreEqual(request.Content.Headers.ContentType.MediaType, "text/plain");

            test_content(request, input);
        }

        [TestMethod]
        public void dictionary_test()
        {
            var request = getEmptyMessage();

            var dic = new Dictionary<string, string> { ["name1"] = "value 1", ["name2"] = "\"'&<>" };
            request.AddContent(dic);

            Assert.AreEqual(request.Content.Headers.ContentType.CharSet, null);
            Assert.AreEqual(request.Content.Headers.ContentType.MediaType, "application/x-www-form-urlencoded");

            test_content(request, "name1=value+1&name2=%22%27%26%3C%3E");
        }

        [TestMethod]
        public void json_test()
        {
            var request = getEmptyMessage();

            var jsonStr = "{\"name1\":\"value 1\"}";
            var json = JObject.Parse(jsonStr);
            request.AddContent(json);

            request.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
            request.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            test_content(request, JObject.Parse(jsonStr).ToString(Newtonsoft.Json.Formatting.Indented));
        }

        HttpRequestMessage getEmptyMessage()
        {
            var request = new HttpRequestMessage();
            Assert.AreEqual(request.Content, null);

            return request;
        }

        void test_content(HttpRequestMessage request, string expectedMessage)
        {
            var contentString = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Assert.AreEqual(expectedMessage, contentString);
        }
    }

    [TestClass]
    public class ParseCookie
    {
        [TestMethod]
        public void null_param_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => SystemNetHttpExtensions.ParseCookie(null));

        [TestMethod]
        public void test_cookie()
        {
            var cookie = SystemNetHttpExtensions.ParseCookie("session-id=139-1488065-0277455; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");
            cookie.Name.Should().Be("session-id");
            cookie.Value.Should().Be("139-1488065-0277455");
            cookie.Domain.Should().Be(".amazon.com");
            cookie.Path.Should().Be("/");
            cookie.Secure.Should().BeFalse();
            cookie.Expires.Should().Be(DateTime.Parse("Thu, 30-Jun-2039 19:07:14 GMT"));
        }
    }

	[TestClass]
	public class ReadAsJObjectAsync
	{
		[TestMethod]
		public async Task valid_FormUrlEncodedContent()
		{
			var message = new HttpResponseMessage
			{
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["k1"] = "v1",
					["k2"] = "!@#$%^&*()<>-=_:'\"\\\n"
				}),
				StatusCode = System.Net.HttpStatusCode.OK
			};

			var str = await message.Content.ReadAsStringAsync();
			str.Should().Be("k1=v1&k2=%21%40%23%24%25%5E%26%2A%28%29%3C%3E-%3D_%3A%27%22%5C%0A");

			var jObj = await message.Content.ReadAsJObjectAsync();
			var json = jObj.ToString(Newtonsoft.Json.Formatting.Indented);
			var expected = @"
{
  ""k1"": ""v1"",
  ""k2"": ""!@#$%^&*()<>-=_:'\""\\\n""
}
		".Trim();

			json.Should().Be(expected);
		}

		[TestMethod]
		public async Task not_supported_HttpContent_type()
		{
			var message = new HttpResponseMessage
			{
				Content = new StreamContent(new MemoryStream()),
				StatusCode = System.Net.HttpStatusCode.OK
			};

			await Assert.ThrowsExceptionAsync<JsonReaderException>(() => message.Content.ReadAsJObjectAsync());
		}

		[TestMethod]
		public async Task invalid_json()
		{
			var message = new HttpResponseMessage
			{
				Content = new StringContent("{\"a\""),
				StatusCode = System.Net.HttpStatusCode.OK
			};
			await Assert.ThrowsExceptionAsync<JsonReaderException>(() => message.Content.ReadAsJObjectAsync());
		}

		[TestMethod]
		public async Task valid_json()
		{
			var message = new HttpResponseMessage
			{
				Content = new StringContent("{'a':1}"),
				StatusCode = System.Net.HttpStatusCode.OK
			};
			var jObj = await message.Content.ReadAsJObjectAsync();
			jObj.ToString(Newtonsoft.Json.Formatting.None).Should().Be("{\"a\":1}");
		}
	}

	[TestClass]
    public class DownloadFileAsync_ISealedHttpClient
    {
        [TestMethod]
        public async Task null_params_throw()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync((ISealedHttpClient)null, "url", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<ISealedHttpClient>().Object, null, "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<ISealedHttpClient>().Object, "url", null));
        }

        [TestMethod]
        public async Task blank_params_throw()
        {
            var mock = new Mock<ISealedHttpClient>().Object;
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "   ", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", "   "));
        }

#if !RUN_LIVE_DOWNLOADS
        [Ignore]
#endif
        [TestMethod]
        public async Task download_small()
        {
            var downloadFileUrl = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf";
            var length = 13264L;
            var md5 = "d41d8cd98f00b204e9800998ecf8427e";

            await downloadTest(downloadFileUrl, length, md5);
        }

#if !RUN_LIVE_DOWNLOADS
        [Ignore]
#endif
        [TestMethod]
        public async Task download_big()
        {
            var downloadFileUrl = "http://www.ovh.net/files/10Mb.dat";
            var length = 1_250_000L;
            var md5 = "d41d8cd98f00b204e9800998ecf8427e";

            await downloadTest(downloadFileUrl, length, md5);
        }

        private static async Task downloadTest(string downloadFileUrl, long length, string md5)
        {
            var temp = Path.GetTempFileName();

            try
            {
                // REAL LIVE download client
                var client = new SealedHttpClient(new HttpClientHandler()) as ISealedHttpClient;
                await client.DownloadFileAsync(downloadFileUrl, temp);

                await Task.Delay(200);

                var fileInfo = new FileInfo(temp);
                Assert.IsTrue(fileInfo.Exists);

                var len = fileInfo.Length;
                len.Should().Be(length);

                var m = fileInfo.MD5();
                m.Should().Be(md5);
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }

    [TestClass]
    public class DownloadFileAsync_HttpClient
    {
        [TestMethod]
        public async Task null_params_throw()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync((HttpClient)null, "url", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<HttpClient>().Object, null, "file"));
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => SystemNetHttpExtensions.DownloadFileAsync(new Mock<HttpClient>().Object, "url", null));
        }

        [TestMethod]
        public async Task blank_params_throw()
        {
            var mock = new Mock<HttpClient>().Object;
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "   ", "file"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => SystemNetHttpExtensions.DownloadFileAsync(mock, "url", "   "));
        }

#if !RUN_LIVE_DOWNLOADS
        [Ignore]
#endif
        [TestMethod]
        public async Task download_small()
        {
            var downloadFileUrl = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf";
            var length = 13264L;
            var md5 = "d41d8cd98f00b204e9800998ecf8427e";

            await downloadTest(downloadFileUrl, length, md5);
        }

#if !RUN_LIVE_DOWNLOADS
        [Ignore]
#endif
        [TestMethod]
        public async Task download_big()
        {
            var downloadFileUrl = "http://www.ovh.net/files/10Mb.dat";
            var length = 1_250_000L;
            var md5 = "d41d8cd98f00b204e9800998ecf8427e";

            await downloadTest(downloadFileUrl, length, md5);
        }

#if !RUN_LIVE_DOWNLOADS
		[Ignore]
#endif
		[TestMethod]
		public async Task download_small_with_progress()
		{
			var downloadFileUrl = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf";
			var length = 13264L;
			var md5 = "d41d8cd98f00b204e9800998ecf8427e";

			var log = new List<string>();

			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (s, e)
				=> log.Add(
					$"{e.BytesReceived}/{e.TotalFileSize} ({e.ProgressPercentage})%"
					);

			var temp = Path.GetTempFileName();
			var destinationFilePath = Path.GetFullPath(temp);

			try
			{
				// REAL LIVE download client
				var client = new HttpClient(new HttpClientHandler());
				await client.DownloadFileAsync(downloadFileUrl, destinationFilePath, progress);

				await Task.Delay(100);

				var fileInfo = new FileInfo(destinationFilePath);
				Assert.IsTrue(fileInfo.Exists);

				var len = fileInfo.Length;
				len.Should().Be(length);

				var m = fileInfo.MD5();
				m.Should().Be(md5);

				log.Count.Should().Be(3);
				log[0].Should().Be("3685/13264 (27.78)%");
				log[1].Should().Be("11877/13264 (89.54)%");
				log[2].Should().Be("13264/13264 (100)%");
			}
			finally
			{
				File.Delete(destinationFilePath);
			}
		}

        private static async Task downloadTest(
			string downloadFileUrl,
			long length,
			string md5)
        {
            var temp = Path.GetTempFileName();

            try
            {
                // REAL LIVE download client
                var client = new HttpClient(new HttpClientHandler());
                await client.DownloadFileAsync(downloadFileUrl, temp);

                await Task.Delay(200);

                var fileInfo = new FileInfo(temp);
                Assert.IsTrue(fileInfo.Exists);

                var len = fileInfo.Length;
                len.Should().Be(length);

                var m = fileInfo.MD5();
                m.Should().Be(md5);
            }
            finally
            {
                File.Delete(temp);
            }
        }
	}
}
