using System;
using System.Collections.Generic;
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
    public static class ApiHelpers
    {
        public static string RemoveTotalResults(this string input)
            => System.Text.RegularExpressions.Regex.Replace(
                input,
                // eg:
                // ,"total_results":40425
                @",\s*""total_results""\s*:\s*\d+", "");
	}

	[TestClass]
	public class ctor_identityMaintainer
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => new Api(new Mock<IIdentityMaintainer>().Object));
	}

    [TestClass]
    public class ctor_identityMaintainer_sharer
	{
		[TestMethod]
		public void null_identity_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new Api(null, new Mock<IClientSharer>().Object));

		[TestMethod]
		public void null_sharer_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new Api(new Mock<IIdentityMaintainer>().Object, null));
	}

	[TestClass]
    public class AdHocNonAuthenticatedGetAsync
    {
        public Api api { get; set; }

        [TestMethod]
        public async Task null_param_throws()
        {
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.AdHocNonAuthenticatedGetAsync(null));
        }

        [TestMethod]
        public async Task empty_param_throws()
        {
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AdHocNonAuthenticatedGetAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AdHocNonAuthenticatedGetAsync("   "));
        }

        [TestMethod]
        public async Task get_product_i_do_not_own()
        {
            var expected = @"
{""product"":{""asin"":""B07B3BCZ9S"",""sku"":""BK_RAND_006061"",""sku_lite"":""BK_RAND_006061""},""response_groups"":[""always-returned"",""categories"",""sku""]}
".Trim();

            api ??= await ApiClientMock.GetApiAsync(expected);

            var pd = "B07B3BCZ9S";
            var url = $"/1.0/catalog/products/{pd}?response_groups=sku,categories";

            var response = await api.AdHocNonAuthenticatedGetAsync(url);
            response.ToString(Formatting.None).Should().Be(expected);
        }

        [TestMethod]
        public async Task get_product_i_own()
        {
            var expected = @"
{""product"":{""asin"":""B01IW9TQPK"",""sku"":""BK_ADBL_027964"",""sku_lite"":""BK_ADBL_027964""},""response_groups"":[""always-returned"",""categories"",""sku""]}
".Trim();

            api ??= await ApiClientMock.GetApiAsync(expected);

            var pd = "B01IW9TQPK";
            var url = $"/1.0/catalog/products/{pd}?response_groups=sku,categories";

            var response = await api.AdHocNonAuthenticatedGetAsync(url);
            response.ToString(Formatting.None).Should().Be(expected);
        }

        [TestMethod]
        public async Task get_products_by_category()
        {
            var start = @"{""product_filters"":[],""products"":[{""asin"":""";
            var end = @"
""}],""response_groups"":[""product_desc"",""always-returned"",""product_extended_attrs"",""contributors"",""price"",""media"",""sku"",""product_attrs"",""sample"",""product_plan_details""]}
".Trim();
            var expected = start + @"
...
  and tons of other stuff
  test with over 600 books returns over 1 million characters of json
...
".Trim() + end;

            api ??= await ApiClientMock.GetApiAsync(expected);

            var catId = "2226658011";
            var url = $"/1.0/catalog/products?category_id={catId}&num_results=3&response_groups=contributors,media,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,sample,sku";

            var response = await api.AdHocNonAuthenticatedGetAsync(url);

            var json = response.ToString(Formatting.None);
            json = json.RemoveTotalResults();
            json.Should().StartWith(start);
            json.Should().EndWith(end);
        }
    }

    [TestClass]
    public class AdHocAuthenticatedGetAsync
    {
        public Api api { get; set; }

        [TestMethod]
        public async Task null_param_throws()
        {
            api ??= await ApiClientMock.GetApiAsync("x");

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.AdHocAuthenticatedGetAsync(null));
        }

        [TestMethod]
        public async Task empty_param_throws()
        {
            api ??= await ApiClientMock.GetApiAsync("x");

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AdHocAuthenticatedGetAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AdHocAuthenticatedGetAsync("   "));
        }

        [TestMethod]
        public async Task get_product_i_do_not_own()
        {
            var expected = @"
{""product"":{""asin"":""B07B3BCZ9S"",""sku"":""BK_RAND_006061"",""sku_lite"":""BK_RAND_006061""},""response_groups"":[""always-returned"",""categories"",""sku""]}
".Trim();

            api ??= await ApiClientMock.GetApiAsync(expected);

            var pd = "B07B3BCZ9S";
            var url = $"/1.0/catalog/products/{pd}?response_groups=sku,categories";

            var response = await api.AdHocAuthenticatedGetAsync(url);
			var str = await response.Content.ReadAsStringAsync();
			str.Should().Be(expected);
		}

        [TestMethod]
        public async Task get_library()
        {
            var jsonStart = "{\"items\":";
            var jsonEnd = "}";

            api ??= await ApiClientMock.GetApiAsync(LibraryFull);

            var url = "/1.0/library?purchaseAfterDate=01/01/1970";

            var response = await api.AdHocAuthenticatedGetAsync(url);
			var str = await response.Content.ReadAsStringAsync();
            str.Should().StartWith(jsonStart);
            str.Should().EndWith(jsonEnd);
        }
    }
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
}
