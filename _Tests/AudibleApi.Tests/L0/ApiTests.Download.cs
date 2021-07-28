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
	public class GetDownloadLicenseAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.GetDownloadLicenseAsync(null));
		}

		[TestMethod]
		public async Task empty_param_throws()
		{
			api ??= await ApiHttpClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadLicenseAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.GetDownloadLicenseAsync("   "));
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

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLicenseAsync("0X0X0X0XXX"));
		}

		[TestMethod]
		public async Task book_not_in_library()
		{
			var msg = @"
{""content_license"":{""acr"":""E1FYA"",""asin"":""172137406X"",""content_metadata"":{},""drm_type"":""Adrm"",""message"":""License not granted to customer [AAAAAAAAAAAAAA] for asin [172137406X]"",""request_id"":""13_S8"",""status_code"":""Denied"",""voucher_id"":""cdn:13_S8""},""response_groups"":[""always-returned""]}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			api ??= await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var license = await api.GetDownloadLicenseAsync("172137406X");
			license.Should().BeNull();
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

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLicenseAsync("B07D84P11M"));
		}
	}
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
	[TestClass]
	public class GetDownloadLicenseAsync_asin
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

			await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.GetDownloadLicenseAsync("172137406X"));
		}

		[TestMethod]
		public async Task book_in_library()
		{
			var asin = "B07L162HDY";
			var voucherJson = "{\"key\":\"00000000000000000000000000000000\",\"iv\":\"11111111111111111111111111111111\",\"refreshDate\":\"2021-06-21T19:52:32Z\",\"removalOnExpirationDate\":\"2021-08-25T19:52:32Z\",\"rules\":[{\"parameters\":[{\"expireDate\":\"2021-07-31T19:52:32Z\",\"type\":\"EXPIRES\"}],\"name\":\"DefaultExpiresRule\"},{\"parameters\":[{\"directedIds\":[\"amzn1.account.AAAAAAAAAAAAAAAAAAAAAAAAAAAA\"],\"type\":\"DIRECTED_IDS\"}],\"name\":\"AllowedUsersRule\"}]}";
			var voucherObj = AudibleApiDTOs.VoucherDtoV10.FromJson(voucherJson);
			voucherObj.Should().NotBeNull();

			var license_response = AudibleApi.Tests.EncryptionHelper.EncryptVoucher(asin, voucherJson);
			var decryptedVoucher = AudibleApi.Tests.EncryptionHelper.DecryptVoucher(asin, license_response);

			decryptedVoucher.Should().BeEquivalentTo(voucherJson, "It was encrypted then decrypted with same parameters.");



			var downloadLink = "ht" + "tps://dc.cloudfront.net/42/bk_2.aax?voucherId=cdn:6G&Policy=eQ__&Key-Pair-Id=AA";
			var msg = $@"
{{""content_license"":{{""acr"":""CRQ"",""asin"":""B07L162HDY"",""content_metadata"":{{""content_url"":{{""offline_url"":""{downloadLink}""}}}},""drm_type"":""Adrm"",""license_response"":""{license_response}"",""message"":""foo"",""request_id"":""66_SG"",""status_code"":""Granted"",""voucher_id"":""cdn:66_SG""}},""response_groups"":[""always-returned""]}}
".Trim();
			var response = new HttpResponseMessage
			{
				Content = new StringContent(msg)
			};

			var api = await ApiHttpClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);
			var license = await api.GetDownloadLicenseAsync(asin);

			license.ContentMetadata.ContentUrl.OfflineUrl.Should().Be(downloadLink);
			license.Voucher.Key.Should().Be(voucherObj.Key);
			license.Voucher.Iv.Should().Be(voucherObj.Iv);
		}
	}
}
