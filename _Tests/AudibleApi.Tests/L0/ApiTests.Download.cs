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
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static TestAudibleApiCommon.ComputedTestValues;

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0
{
	[TestClass]
	public class GetDownloadLinkAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.GetDownloadLinkAsync(null));
		}

		[TestMethod]
		public async Task empty_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadLinkAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadLinkAsync("   "));
		}

		[TestMethod]
		public async Task book_not_found()
		{
			var msg = "{\"message\":\"Requested ASIN not found.\"}";
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			api ??= await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLinkAsync("0X0X0X0XXX"));
		}

		[TestMethod]
		public async Task book_not_in_library()
		{
			var msg = @"
{""content_license"":{""acr"":""E1FYA"",""asin"":""172137406X"",""content_metadata"":{},""drm_type"":""Adrm"",""message"":""Client does not have plans that support asin benefits.]]. Licensing details:[License not granted]"",""request_id"":""13_S8"",""status_code"":""Denied"",""voucher_id"":""cdn:13_S8""},""response_groups"":[""always-returned""]}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			api ??= await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var link = await api.GetDownloadLinkAsync("172137406X");
			link.Should().BeNull();
		}

		[TestMethod]
		public async Task multi_part_book_parent_error()
		{
			// when attempting to download a huge book (eg: complete sherlock holmes, super powereds year 4) you can't use the main book's asin. get the asin of each part from:
			// /1.0/library/{asin}?response_groups=relationships
			// attempting to d/l using the parent asin will result in the below error
			var msg = @"
{
  ""error_code"": ""000307"",
  ""message"": ""Unable to retrieve asset details from Sable(CDN), fallback to CDS for customerId:AA, marketplaceId:AA, asin:B07D84P11M, acr:null, skuLite:BK_TANT_011418, aaaClientId:ApolloEnv:AudibleApiExternalRouterService/Prod""
}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			api ??= await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLinkAsync("B07D84P11M"));
		}
	}

	[TestClass]
	public class DownloadPartAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_params_throw()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.DownloadPartAsync("foo", null));
			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.DownloadPartAsync(null, "foo"));
		}

		[TestMethod]
		public async Task blank_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DownloadPartAsync("", "foo"));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DownloadPartAsync("   ", "foo"));

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DownloadPartAsync("foo", ""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DownloadPartAsync("foo", "   "));
		}

		[TestMethod]
		public async Task no_download_link()
		{
			var msg = @"
{""content_license"":{""acr"":""E1FYA"",""asin"":""172137406X"",""content_metadata"":{},""drm_type"":""Adrm"",""message"":""License not granted"",""request_id"":""13_S8"",""status_code"":""Denied"",""voucher_id"":""cdn:13_S8""},""response_groups"":[""always-returned""]}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			api ??= await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var temp = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				Guid.NewGuid().ToString() + ".aax");

			var asinNotInLibrary = "172137406X";

			var filename = await api.DownloadPartAsync(asinNotInLibrary, temp);
			filename.Should().BeNull();
			await Task.Delay(100);
			System.IO.File.Exists(temp).Should().BeFalse();
		}

		// actually an error in GetDownloadLinkAsync. see its unit tests
		// public async Task multi_part_book_failure()
	}

	[TestClass]
	public class GetDownloadablePartsAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.GetDownloadablePartsAsync(null));
		}

		[TestMethod]
		public async Task blank_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadablePartsAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadablePartsAsync("   "));
		}
	}

	[TestClass]
	public class DownloadAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_asin_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(
				() => api.DownloadAsync(null, "file"));
		}

		[TestMethod]
		public async Task blank_asin_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAsync("", "file"));
			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAsync("   ", "file"));
		}

		[TestMethod]
		public async Task null_file_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(
				() => api.DownloadAsync("asin", null));
		}

		[TestMethod]
		public async Task blank_file_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAsync("asin", ""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAsync("asin", "   "));
		}

		// see details in DownloadPartAsync.download_success()
		// for explanation of download url and download bytes steps
	}

	[TestClass]
	public class DownloadAaxWorkaroundAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_asin_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(
				() => api.DownloadAaxWorkaroundAsync(null, "file"));
		}

		[TestMethod]
		public async Task blank_asin_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAaxWorkaroundAsync("", "file"));
			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAaxWorkaroundAsync("   ", "file"));
		}

		[TestMethod]
		public async Task null_file_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(
				() => api.DownloadAaxWorkaroundAsync("asin", null));
		}

		[TestMethod]
		public async Task blank_file_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAaxWorkaroundAsync("asin", ""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(
				() => api.DownloadAaxWorkaroundAsync("asin", "   "));
		}
	}
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
	[TestClass]
	public class GetDownloadLinkAsync
	{
		[TestMethod]
		public async Task unknown_status_code()
		{
			var msg = @"
{""message"":""bar"",""content_license"":{""content_metadata"":{},""status_code"":""Foo""},""response_groups"":[""always-returned""]}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			var api = await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLinkAsync("172137406X"));
		}

		[TestMethod]
		public async Task book_in_library()
		{
			var downloadLink = "ht" + "tps://dc.cloudfront.net/42/bk_2.aax?voucherId=cdn:6G&Policy=eQ__&Key-Pair-Id=AA";
			var msg = $@"
{{""content_license"":{{""acr"":""CRQ"",""asin"":""B07L162HDY"",""content_metadata"":{{""content_url"":{{""offline_url"":""{downloadLink}""}}}},""drm_type"":""Adrm"",""license_response"":""I="",""message"":""foo"",""request_id"":""66_SG"",""status_code"":""Granted"",""voucher_id"":""cdn:66_SG""}},""response_groups"":[""always-returned""]}}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			var api = await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var link = await api.GetDownloadLinkAsync("B07L162HDY");
			link.Should().Be(downloadLink);
		}
	}

	[TestClass]
	public class DownloadPartAsync
	{
		[TestMethod]
		public async Task download_success()
		{
			var badExt = ".xyz";
			var goodExt = ".aax";

			// RESPONSE 1
			var downloadLink = "ht" + $"tps://dc.cloudfront.net/42/bk_2{goodExt}?voucherId=cdn:6G&Policy=eQ__&Key-Pair-Id=AA";
			var msg = $@"
{{""content_license"":{{""acr"":""CQ"",""asin"":""B07L162HDY"",""content_metadata"":{{""content_url"":{{""offline_url"":""{downloadLink}""}}}},""drm_type"":""Adrm"",""license_response"":""I="",""message"":""Eligibility details"",""request_id"":""66_SG"",""status_code"":""Granted"",""voucher_id"":""cdn:66_SG""}},""response_groups"":[""always-returned""]}}
".Trim();
			var linkResponse = new HttpResponseMessage
			{ Content = new StringContent(msg), RequestMessage = new HttpRequestMessage() };

			// RESPONSE 2
			// base 64 of a txt file with "test"
			var base64 = "dGVzdA==";
			var bytes = Convert.FromBase64String(base64);
			var md5 = "d41d8cd98f00b204e9800998ecf8427e";
			var fileBytesResponse = new HttpResponseMessage
			{ Content = new ByteArrayContent(bytes), RequestMessage = new HttpRequestMessage() };

			var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
			handlerMock.Protected()
				.SetupSequence<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				// 1st return: download link
				.ReturnsAsync(linkResponse)
				// 2nd return: file bytes
				.ReturnsAsync(fileBytesResponse);

			var api = await ApiHttpClientMock.GetApiAsync(handlerMock.Object);

			var path = System.IO.Path.GetTempPath();
			var filenameNoExt = Guid.NewGuid().ToString();
			var badFileName = System.IO.Path.Combine(path, filenameNoExt + badExt);
			var goodFileName = System.IO.Path.Combine(path, filenameNoExt + goodExt);

			try
			{
				await api.DownloadPartAsync("B07L162HDY", badFileName);
				await Task.Delay(100);

				// verify file "download", incl corrected extension
				System.IO.File.Exists(goodFileName);
				var fileinfo = new System.IO.FileInfo(goodFileName);
				fileinfo.MD5().Should().Be(md5);
			}
			finally
			{
				System.IO.File.Delete(goodFileName);
			}
		}
	}

	[TestClass]
	public class GetDownloadablePartsAsync
	{
		[TestMethod]
		public async Task _1_part()
		{
			var harryPotterAsin = "B017V4IM1G";
			var bookDetails = @"
{
  ""item"": {
    ""asin"": ""B017V4IM1G"",
    ""relationships"": [
      {
        ""asin"": ""B07CM5ZDJL"",
        ""relationship_to_product"": ""parent"",
        ""relationship_type"": ""series"",
        ""sequence"": ""1"",
        ""sku"": ""SE_RIES_027761"",
        ""sku_lite"": ""SE_RIES_027761"",
        ""sort"": ""1""
      },
      {
        ""asin"": ""B0182NWM9I"",
        ""relationship_to_product"": ""parent"",
        ""relationship_type"": ""series"",
        ""sequence"": ""1"",
        ""sku"": ""SE_RIES_013041"",
        ""sku_lite"": ""SE_RIES_013041"",
        ""sort"": ""1""
      }
    ],
    ""sku_lite"": ""BK_POTR_000001"",
    ""status"": ""Active""
  },
  ""response_groups"": [
    ""relationships"",
    ""always-returned""
  ]
}
".Trim();
			var api = await ApiHttpClientMock.GetApiAsync(bookDetails);

			var partsIEnum = await api.GetDownloadablePartsAsync(harryPotterAsin);

			var parts = partsIEnum.ToList();
			parts.Count.Should().Be(1);
			parts[0].Should().Be(harryPotterAsin);
		}

		[TestMethod]
		public async Task _6_parts()
		{
			var sherlockHolmesAsin = "B06WLMWF2S";
			var part1 = "B06WWG59CP";
			var part2 = "B06WVKGR8M";
			var part3 = "B06WW37874";
			var part4 = "B06WW57B11";
			var part5 = "B06WVVVBW5";
			var part6 = "B06WVVVC7T";
			var bookDetails = $@"
{{
  ""item"": {{
    ""asin"": ""B06WLMWF2S"",
    ""relationships"": [
      {{
        ""asin"": ""{part1}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318a"",
        ""sku_lite"": ""BK_ADBL_030318a"",
        ""sort"": ""1""
      }},
      {{
        ""asin"": ""{part2}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318b"",
        ""sku_lite"": ""BK_ADBL_030318b"",
        ""sort"": ""2""
      }},
      {{
        ""asin"": ""{part4}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318d"",
        ""sku_lite"": ""BK_ADBL_030318d"",
        ""sort"": ""4""
      }},
      {{
        ""asin"": ""{part6}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318f"",
        ""sku_lite"": ""BK_ADBL_030318f"",
        ""sort"": ""6""
      }},
      {{
        ""asin"": ""{part5}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318e"",
        ""sku_lite"": ""BK_ADBL_030318e"",
        ""sort"": ""5""
      }},
      {{
        ""asin"": ""{part3}"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318c"",
        ""sku_lite"": ""BK_ADBL_030318c"",
        ""sort"": ""3""
      }}
    ],
    ""sku_lite"": ""BK_ADBL_030318""
  }},
  ""response_groups"": [
    ""relationships"",
    ""always-returned""
  ]
}}
".Trim();
			var api = await ApiHttpClientMock.GetApiAsync(bookDetails);

			var partsIEnum = await api.GetDownloadablePartsAsync(sherlockHolmesAsin);

			var parts = partsIEnum.ToList();
			parts.Count.Should().Be(6);
			parts[0].Should().Be(part1);
			parts[1].Should().Be(part2);
			parts[2].Should().Be(part3);
			parts[3].Should().Be(part4);
			parts[4].Should().Be(part5);
			parts[5].Should().Be(part6);
		}
	}

	[TestClass]
	public class DownloadAsync
	{
		[TestMethod]
		public async Task download_1()
		{
			var bookDetails = @"
{
  ""item"": {
    ""asin"": ""B017V4IM1G"",
    ""relationships"": [
      {
        ""asin"": ""B07CM5ZDJL"",
        ""relationship_to_product"": ""parent"",
        ""relationship_type"": ""series"",
        ""sequence"": ""1"",
        ""sku"": ""SE_RIES_027761"",
        ""sku_lite"": ""SE_RIES_027761"",
        ""sort"": ""1""
      },
      {
        ""asin"": ""B0182NWM9I"",
        ""relationship_to_product"": ""parent"",
        ""relationship_type"": ""series"",
        ""sequence"": ""1"",
        ""sku"": ""SE_RIES_013041"",
        ""sku_lite"": ""SE_RIES_013041"",
        ""sort"": ""1""
      }
    ],
    ""sku_lite"": ""BK_POTR_000001"",
    ""status"": ""Active""
  },
  ""response_groups"": [
    ""relationships"",
    ""always-returned""
  ]
}
".Trim();
			var api = await setupDownloads(bookDetails, 1);

			var path = System.IO.Path.GetTempPath();
			var filenameNoExt = Guid.NewGuid().ToString();
			var fileName = System.IO.Path.Combine(path, filenameNoExt + ".aax");

			try
			{
				var downloadedFiles = (await api.DownloadAsync("B017V4IM1G", fileName)).ToList();
				await Task.Delay(100);

				downloadedFiles.Count.Should().Be(1);
				downloadedFiles[0].Should().Be(fileName);

				System.IO.File.Exists(fileName);
			}
			finally
			{
				System.IO.File.Delete(fileName);
			}
		}

		[TestMethod]
		public async Task download_6()
		{
			var bookDetails = @"
{
  ""item"": {
    ""asin"": ""B06WLMWF2S"",
    ""relationships"": [
      {
        ""asin"": ""B06WWG59CP"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318a"",
        ""sku_lite"": ""BK_ADBL_030318a"",
        ""sort"": ""1""
      },
      {
		""asin"": ""B06WVKGR8M"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318b"",
        ""sku_lite"": ""BK_ADBL_030318b"",
        ""sort"": ""2""
      },
      {
		""asin"": ""B06WW57B11"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318d"",
        ""sku_lite"": ""BK_ADBL_030318d"",
        ""sort"": ""4""
      },
      {
		""asin"": ""B06WVVVC7T"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318f"",
        ""sku_lite"": ""BK_ADBL_030318f"",
        ""sort"": ""6""
      },
      {
		""asin"": ""B06WVVVBW5"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318e"",
        ""sku_lite"": ""BK_ADBL_030318e"",
        ""sort"": ""5""
      },
      {
		""asin"": ""B06WW37874"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318c"",
        ""sku_lite"": ""BK_ADBL_030318c"",
        ""sort"": ""3""
      }
    ],
    ""sku_lite"": ""BK_ADBL_030318""
  },
  ""response_groups"": [
    ""relationships"",
    ""always-returned""
  ]
}
".Trim();
			var api = await setupDownloads(bookDetails, 6);

			var path = System.IO.Path.GetTempPath();
			var filenameNoExt = Guid.NewGuid().ToString();
			var inputFileName = System.IO.Path.Combine(path, filenameNoExt + ".aax");

			var fileName1 = System.IO.Path.Combine(path, filenameNoExt + "(1).aax");
			var fileName2 = System.IO.Path.Combine(path, filenameNoExt + "(2).aax");
			var fileName3 = System.IO.Path.Combine(path, filenameNoExt + "(3).aax");
			var fileName4 = System.IO.Path.Combine(path, filenameNoExt + "(4).aax");
			var fileName5 = System.IO.Path.Combine(path, filenameNoExt + "(5).aax");
			var fileName6 = System.IO.Path.Combine(path, filenameNoExt + "(6).aax");

			try
			{
				var downloadedFiles = (await api.DownloadAsync("B06WLMWF2S", inputFileName)).ToList();
				await Task.Delay(100);

				downloadedFiles.Count.Should().Be(6);
				downloadedFiles[0].Should().Be(fileName1);
				downloadedFiles[1].Should().Be(fileName2);
				downloadedFiles[2].Should().Be(fileName3);
				downloadedFiles[3].Should().Be(fileName4);
				downloadedFiles[4].Should().Be(fileName5);
				downloadedFiles[5].Should().Be(fileName6);

				// verify file "downloads"
				System.IO.File.Exists(fileName1);
				System.IO.File.Exists(fileName2);
				System.IO.File.Exists(fileName3);
				System.IO.File.Exists(fileName4);
				System.IO.File.Exists(fileName5);
				System.IO.File.Exists(fileName6);
			}
			finally
			{
				System.IO.File.Delete(fileName1);
				System.IO.File.Delete(fileName2);
				System.IO.File.Delete(fileName3);
				System.IO.File.Delete(fileName4);
				System.IO.File.Delete(fileName5);
				System.IO.File.Delete(fileName6);
			}
		}

		[TestMethod]
		public async Task download_10_has_leading_zeros()
		{
			var bookDetails = @"
{
  ""item"": {
    ""asin"": ""B06WLMWF2S"",
    ""relationships"": [
      {
        ""asin"": ""B06WWG59CP"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318a"",
        ""sku_lite"": ""BK_ADBL_030318a"",
        ""sort"": ""1""
      },
      {
		""asin"": ""B06WVKGR8M"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318b"",
        ""sku_lite"": ""BK_ADBL_030318b"",
        ""sort"": ""2""
      },
      {
		""asin"": ""B06WW57B11"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318d"",
        ""sku_lite"": ""BK_ADBL_030318d"",
        ""sort"": ""4""
      },
      {
		""asin"": ""B06WVVVC7T"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318f"",
        ""sku_lite"": ""BK_ADBL_030318f"",
        ""sort"": ""6""
      },
      {
		""asin"": ""B06WVVVBW5"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318e"",
        ""sku_lite"": ""BK_ADBL_030318e"",
        ""sort"": ""5""
      },
      {
		""asin"": ""B06WW37874"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318c"",
        ""sku_lite"": ""BK_ADBL_030318c"",
        ""sort"": ""3""
      },
      {
		""asin"": ""foo7"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318g"",
        ""sku_lite"": ""BK_ADBL_030318g"",
        ""sort"": ""7""
      },
      {
		""asin"": ""foo8"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318h"",
        ""sku_lite"": ""BK_ADBL_030318h"",
        ""sort"": ""8""
      },
      {
		""asin"": ""foo9"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318i"",
        ""sku_lite"": ""BK_ADBL_030318i"",
        ""sort"": ""9""
      },
      {
		""asin"": ""foo10"",
        ""relationship_to_product"": ""child"",
        ""relationship_type"": ""component"",
        ""sku"": ""BK_ADBL_030318j"",
        ""sku_lite"": ""BK_ADBL_030318j"",
        ""sort"": ""10""
      }
    ],
    ""sku_lite"": ""BK_ADBL_030318""
  },
  ""response_groups"": [
    ""relationships"",
    ""always-returned""
  ]
}
".Trim();
			var api = await setupDownloads(bookDetails, 10);

			var path = System.IO.Path.GetTempPath();
			var filenameNoExt = Guid.NewGuid().ToString();
			var inputFileName = System.IO.Path.Combine(path, filenameNoExt + ".aax");

			var fileName1 = System.IO.Path.Combine(path, filenameNoExt + "(01).aax");
			var fileName2 = System.IO.Path.Combine(path, filenameNoExt + "(02).aax");
			var fileName3 = System.IO.Path.Combine(path, filenameNoExt + "(03).aax");
			var fileName4 = System.IO.Path.Combine(path, filenameNoExt + "(04).aax");
			var fileName5 = System.IO.Path.Combine(path, filenameNoExt + "(05).aax");
			var fileName6 = System.IO.Path.Combine(path, filenameNoExt + "(06).aax");
			var fileName7 = System.IO.Path.Combine(path, filenameNoExt + "(07).aax");
			var fileName8 = System.IO.Path.Combine(path, filenameNoExt + "(08).aax");
			var fileName9 = System.IO.Path.Combine(path, filenameNoExt + "(09).aax");
			var fileName10 = System.IO.Path.Combine(path, filenameNoExt + "(10).aax");

			try
			{
				var downloadedFiles = (await api.DownloadAsync("B06WLMWF2S", inputFileName)).ToList();
				await Task.Delay(100);

				downloadedFiles.Count.Should().Be(10);
				downloadedFiles[0].Should().Be(fileName1);
				downloadedFiles[1].Should().Be(fileName2);
				downloadedFiles[2].Should().Be(fileName3);
				downloadedFiles[3].Should().Be(fileName4);
				downloadedFiles[4].Should().Be(fileName5);
				downloadedFiles[5].Should().Be(fileName6);
				downloadedFiles[6].Should().Be(fileName7);
				downloadedFiles[7].Should().Be(fileName8);
				downloadedFiles[8].Should().Be(fileName9);
				downloadedFiles[9].Should().Be(fileName10);

				// verify file "downloads"
				System.IO.File.Exists(fileName1);
				System.IO.File.Exists(fileName2);
				System.IO.File.Exists(fileName3);
				System.IO.File.Exists(fileName4);
				System.IO.File.Exists(fileName5);
				System.IO.File.Exists(fileName6);
				System.IO.File.Exists(fileName7);
				System.IO.File.Exists(fileName8);
				System.IO.File.Exists(fileName9);
				System.IO.File.Exists(fileName10);
			}
			finally
			{
				System.IO.File.Delete(fileName1);
				System.IO.File.Delete(fileName2);
				System.IO.File.Delete(fileName3);
				System.IO.File.Delete(fileName4);
				System.IO.File.Delete(fileName5);
				System.IO.File.Delete(fileName6);
				System.IO.File.Delete(fileName7);
				System.IO.File.Delete(fileName8);
				System.IO.File.Delete(fileName9);
				System.IO.File.Delete(fileName10);
			}
		}

		private async Task<Api> setupDownloads(string bookDetailsJson, int qtyBooks)
		{
			var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);

			// RESPONSE 1: get part(s)
			var bookDetailsResponse = new HttpResponseMessage
			{ Content = new StringContent(bookDetailsJson), RequestMessage = new HttpRequestMessage() };

			var setupSeqResult = handlerMock.Protected()
				.SetupSequence<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				// RESPONSE 1: get part(s)
				.ReturnsAsync(bookDetailsResponse);

			for (var i = 0; i < qtyBooks; i++)
			{
				// RESPONSE 2: get download url
				var downloadLink = "ht" + $"tps://dc.cloudfront.net/42/bk_2.aax?voucherId=cdn:6G&Policy=eQ__&Key-Pair-Id=AA";
				var msg = $@"
{{""content_license"":{{""acr"":""CQ"",""asin"":""B07L162HDY"",""content_metadata"":{{""content_url"":{{""offline_url"":""{downloadLink}""}}}},""drm_type"":""Adrm"",""license_response"":""I="",""message"":""Eligibility details"",""request_id"":""66_SG"",""status_code"":""Granted"",""voucher_id"":""cdn:66_SG""}},""response_groups"":[""always-returned""]}}
".Trim();
				var linkResponse = new HttpResponseMessage
				{ Content = new StringContent(msg), RequestMessage = new HttpRequestMessage() };

				// RESPONSE 3: download bytes
				var base64 = "dGVzdA==";
				var bytes = Convert.FromBase64String(base64);
				var fileBytesResponse = new HttpResponseMessage
				{ Content = new ByteArrayContent(bytes), RequestMessage = new HttpRequestMessage() };

				setupSeqResult
					= setupSeqResult
					// RESPONSE 2: get download url
					.ReturnsAsync(linkResponse)
					// RESPONSE 3: download bytes
					.ReturnsAsync(fileBytesResponse);
			}

			var api = await ApiHttpClientMock.GetApiAsync(handlerMock.Object);
			return api;
		}
	}

	[TestClass]
	public class DownloadAaxWorkaroundAsync
	{
		[TestMethod]
		public async Task status_code_fails()
		{
			var mock = new ApiCallTester("tester returns StatusCode OK");
			var api = mock.Api;
			await Assert.ThrowsExceptionAsync<Exception>(() => api.DownloadAaxWorkaroundAsync("asin", "file"));
		}

		class myLazyMockHandler : HttpClientHandler
		{
			public HttpResponseMessage _1stResponse;
			public HttpResponseMessage _2ndResponse;

			public HttpRequestMessage _2ndRequest { get; private set; }

			int count = 0;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				count++;

				if (count == 1)
					return Task.FromResult(_1stResponse);

				if (count == 2)
				{
					_2ndRequest = request;
					return Task.FromResult(_2ndResponse);
				}

				throw new NotImplementedException();
			}
		}

		//1st
		//  SendAsync(HttpRequestMessage)
		//    response.StatusCode != HttpStatusCode.Found
		//    origDownloadLink = response.Headers.Location.AbsoluteUri;
		//
		// localize
		//
		//2nd
		//  GetAsync(string localizedDownloadLink, HttpCompletionOption)

		[TestMethod]
		public async Task omnibus_unit_test()
		{
			// single test for many intertwined parts
			// - download link returned
			// - download link localized
			// - 2nd response w/filename
			// - returned final filename
			// - verify file created

			var expectedFinalFilename = "file.aax";
			try
			{
				Localization.SetLocale(Localization.LocaleNames.UK);

				// RESPONSE 1: download link
				var linkResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.Found };
				linkResponse.Headers.Location = new Uri("https://cds.audible.com/downloadme?a=1");

				var downloadResponse = new HttpResponseMessage
				{ Content = new ByteArrayContent(Convert.FromBase64String("dGVzdA==")) };
				downloadResponse.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "foo_ep6.aax" };

				var lazyMockHandler = new myLazyMockHandler
				{
					_1stResponse = linkResponse,
					_2ndResponse = downloadResponse
				};

				var api = await ApiHttpClientMock.GetApiAsync(lazyMockHandler);
				var finalFile = await api.DownloadAaxWorkaroundAsync("asin", "file.xyz");

				finalFile.Should().Be(expectedFinalFilename);

				File.Exists(expectedFinalFilename).Should().BeTrue();

				lazyMockHandler._2ndRequest
					.RequestUri.AbsoluteUri
					.Should().Be("https://cds.audible.co.uk/downloadme?a=1");
			}
			finally
			{
				await Task.Delay(100);

				Localization.SetLocale(Localization.LocaleNames.US);
				if (File.Exists(expectedFinalFilename))
					File.Delete(expectedFinalFilename);
			}
		}
	}
}
