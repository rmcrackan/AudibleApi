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
	[TestClass]
	public class GetLibraryAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task verify_response()
		{
			var json = await GetResponseAsync();

			var jsonStart = "{\"items\":";
			var jsonEnd = @"
""response_groups"":[""always-returned"",""product_desc"",""contributors"",""pdf_url"",""category_ladders"",""media"",""product_attrs""]}
".Trim();
			json.Should().StartWith(jsonStart);
			json.Should().EndWith(jsonEnd);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public async Task<string> GetResponseAsync()
		{
			api ??= await ApiHttpClientMock.GetApiAsync(LibraryFull);

			var response = await api.GetLibraryAsync();
			var json = response.ToString(Formatting.None);
			return json;
		}
	}

	[TestClass]
	public class GetLibraryAsync_libraryOptions
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task verify_response()
		{
			var json = await GetResponseAsync();

			var jsonStart = "{\"items\":";
			var jsonEnd = @"
""response_groups"":[""always-returned"",""reviews"",""sku""]}
".Trim();
			json.Should().StartWith(jsonStart);
			json.Should().EndWith(jsonEnd);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public async Task<string> GetResponseAsync()
		{
			var libraryOptions = new LibraryOptions
			{
				NumberOfResultPerPage = 20,
				PageNumber = 5,
				PurchasedAfter = new DateTime(1970, 1, 1),
				ResponseGroups = LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Reviews,
				SortBy = LibraryOptions.SortByOptions.TitleDesc
			};

			api ??= await ApiHttpClientMock.GetApiAsync(LibraryWithOptions);

			var response = await api.GetLibraryAsync(libraryOptions);
			var json = response.ToString(Formatting.None);
			return json;
		}
	}

	[TestClass]
	public class GetLibraryBookAsync_responseGroups
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task verify_response()
		{
			var json = await GetResponseAsync();

			var jsonStart = "{\"item\":";
			var jsonEnd = @"
""response_groups"":[""relationships"",""always-returned"",""product_attrs""]}
".Trim();
			json.Should().StartWith(jsonStart);
			json.Should().EndWith(jsonEnd);
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public async Task<string> GetResponseAsync()
		{
			api ??= await ApiHttpClientMock.GetApiAsync(LibraryBookWithResponseGroups);

			var harryPotterAsin = "B017V4IM1G";
			var response = await api.GetLibraryBookAsync(harryPotterAsin, LibraryOptions.ResponseGroupOptions.ProductAttrs | LibraryOptions.ResponseGroupOptions.Relationships);
			var json = response.ToString(Formatting.None);
			return json;
		}
	}
}

namespace LibraryOptionsTests
{
	[TestClass]
	public class NumberOfResultPerPage
	{
		[TestMethod]
		public void out_of_range_throws()
		{
			var libraryOptions = new LibraryOptions();
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.NumberOfResultPerPage = -1);
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.NumberOfResultPerPage = 0);
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.NumberOfResultPerPage = 1001);
		}

		[TestMethod]
		public void in_range()
		{
			var libraryOptions = new LibraryOptions();

			libraryOptions.NumberOfResultPerPage = null;
			libraryOptions.NumberOfResultPerPage.Should().BeNull();

			libraryOptions.NumberOfResultPerPage = 1;
			libraryOptions.NumberOfResultPerPage.Should().Be(1);

			libraryOptions.NumberOfResultPerPage = 1000;
			libraryOptions.NumberOfResultPerPage.Should().Be(1000);
		}
	}

	[TestClass]
	public class PageNumber
	{
		[TestMethod]
		public void out_of_range_throws()
		{
			var libraryOptions = new LibraryOptions();
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.PageNumber = 0);
		}

		[TestMethod]
		public void in_range()
		{
			var libraryOptions = new LibraryOptions();

			libraryOptions.PageNumber = null;
			libraryOptions.PageNumber.Should().BeNull();

			libraryOptions.PageNumber = 1;
			libraryOptions.PageNumber.Should().Be(1);

			libraryOptions.PageNumber = 1000;
			libraryOptions.PageNumber.Should().Be(1000);
		}
	}

	[TestClass]
	public class PurchasedAfter
	{
		[TestMethod]
		public void in_range()
		{
			var libraryOptions = new LibraryOptions();

			libraryOptions.PurchasedAfter = null;
			libraryOptions.PurchasedAfter.Should().BeNull();

			var now = DateTime.Now;
			libraryOptions.PurchasedAfter = now;
			libraryOptions.PurchasedAfter.Should().Be(now);

			libraryOptions.PurchasedAfter = DateTime.MinValue;
			libraryOptions.PurchasedAfter.Should().Be(DateTime.MinValue);

			libraryOptions.PurchasedAfter = DateTime.MaxValue;
			libraryOptions.PurchasedAfter.Should().Be(DateTime.MaxValue);
		}
	}

	[TestClass]
	public class ToQueryString
	{
		[TestMethod]
		public void default_returns_empty()
			=> new LibraryOptions().ToQueryString().Should().BeEmpty();

		[TestMethod]
		public void only_NumberOfResultPerPage()
		{
			var libraryOptions = new LibraryOptions
			{
				NumberOfResultPerPage = 20
			};
			var expected = "num_results=20";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void only_PageNumber()
		{
			var libraryOptions = new LibraryOptions
			{
				PageNumber = 5
			};
			var expected = "page=5";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void only_PurchasedAfter()
		{
			var libraryOptions = new LibraryOptions
			{
				PurchasedAfter = new DateTime(1970, 1, 1)
			};
			var expected = "purchased_after=1970-01-01T00:00:00Z";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void only_ResponseGroups()
		{
			var libraryOptions = new LibraryOptions
			{
				ResponseGroups = LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Reviews
			};
			var expected = "response_groups=reviews,sku";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void only_SortBy()
		{
			var libraryOptions = new LibraryOptions
			{
				SortBy = LibraryOptions.SortByOptions.TitleDesc
			};
			var expected = "sort_by=-Title";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void _2_properties()
		{
			var libraryOptions = new LibraryOptions
			{
				NumberOfResultPerPage = 20,
				PageNumber = 5
			};

			var expected = "num_results=20&page=5";
			libraryOptions.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void all()
		{
			var libraryOptions = new LibraryOptions
			{
				NumberOfResultPerPage = 20,
				PageNumber = 5,
				PurchasedAfter = new DateTime(1970, 1, 1),
				ResponseGroups = LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Reviews,
				SortBy = LibraryOptions.SortByOptions.TitleDesc
			};

			var expected
				= "num_results=20"
				+ "&page=5"
				+ "&purchased_after=1970-01-01T00:00:00Z"
				+ "&response_groups=reviews,sku"
				+ "&sort_by=-Title";
			libraryOptions.ToQueryString().Should().Be(expected);
		}
	}
}

namespace LibraryOptions_ResponseGroupOptions_Tests
{
	[TestClass]
	public class ToQueryString
	{
		[TestMethod]
		public void invalid_groups_throws()
		{
			var responseGroups = (LibraryOptions.ResponseGroupOptions)(1 << 31);
			Assert.ThrowsException<Exception>(() => responseGroups.ToQueryString());
		}

		[TestMethod]
		public void None_returns_empty()
			=> LibraryOptions.ResponseGroupOptions.None
			.ToQueryString()
			.Should().BeEmpty();

		[TestMethod]
		public void parse_1()
		{
			var responseGroups = LibraryOptions.ResponseGroupOptions.Sku;
			var expected = "response_groups=sku";
			responseGroups.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void parse_multiple()
		{
			var responseGroups = LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Reviews;
			var expected = "response_groups=reviews,sku";
			responseGroups.ToQueryString().Should().Be(expected);
		}

		[TestMethod]
		public void parse_all()
		{
			var responseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
			var expected = "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";
			responseGroups.ToQueryString().Should().Be(expected);
		}
	}
}

namespace LibraryOptions_SortByOptions_Tests
{
	[TestClass]
	public class ToQueryString
	{
		[TestMethod]
		public void invalid_SortBy_throws()
		{
			var libraryOptions = new LibraryOptions
			{
				SortBy = (LibraryOptions.SortByOptions)(1 << 31)
			};
			Assert.ThrowsException<Exception>(() => libraryOptions.ToQueryString());
		}

		[TestMethod]
		public void None_returns_empty()
			=> LibraryOptions.SortByOptions.None
			.ToQueryString()
			.Should().BeEmpty();

		[TestMethod]
		public void parse_valid()
			=> LibraryOptions.SortByOptions.TitleDesc
			.ToQueryString()
			.Should().Be("sort_by=-Title");
	}
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
	[TestClass]
	public class GetLibraryAsync
	{
		[TestMethod]
		public async Task verify_request_string()
		{
			var tester = new ApiCallTester(LibraryFull);
			await tester.Api.GetLibraryAsync();

			var expected = "/1.0/library?purchased_after=1970-01-01T00:00:00Z";
			tester.CapturedRequest
				.RequestUri.OriginalString
				.Should().Be(expected);
		}
	}

	[TestClass]
	public class GetLibraryAsync_libraryOptions
	{
		const string LibDefault = "/1.0/library?purchased_after=1970-01-01T00:00:00Z";

		[TestMethod]
		public async Task null_returns_default()
			=> await test(null, LibDefault);

		[TestMethod]
		public async Task use_all_options()
		{
			var libraryOptions = new LibraryOptions
			{
				NumberOfResultPerPage = 20,
				PageNumber = 5,
				PurchasedAfter = new DateTime(1970, 1, 1),
				ResponseGroups = LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Reviews,
				SortBy = LibraryOptions.SortByOptions.TitleDesc
			};

			var expected
				= "/1.0/library?"
				+ "num_results=20"
				+ "&page=5"
				+ "&purchased_after=1970-01-01T00:00:00Z"
				+ "&response_groups=reviews,sku"
				+ "&sort_by=-Title";

			await test(libraryOptions, expected);
		}

		private static async Task test(LibraryOptions libraryOptions, string expected)
		{
			var tester = new ApiCallTester(LibraryFull);
			await tester.Api.GetLibraryAsync(libraryOptions);

			tester.CapturedRequest
				.RequestUri.OriginalString
				.Should().Be(expected);
		}
	}

	[TestClass]
	public class GetLibraryAsync_string
	{
		const string LibDefault = "/1.0/library?purchased_after=1970-01-01T00:00:00Z";

		[TestMethod]
		public async Task null_returns_default()
			=> await test(null, LibDefault);

		[TestMethod]
		public async Task empty_returns_default()
			=> await test("", LibDefault);

		[TestMethod]
		public async Task whitespace_returns_default()
			=> await test("   ", LibDefault);

		[TestMethod]
		public async Task question_returns_default()
			=> await test(" ? ", LibDefault);

		[TestMethod]
		public async Task accepts_question_prefix()
			=> await test("?page=1", "/1.0/library?page=1");

		[TestMethod]
		public async Task accepts_no_question_prefix()
			=> await test("page=1", "/1.0/library?page=1");

		private static async Task test(string parameters, string expected)
		{
			var tester = new ApiCallTester(LibraryFull);
			await tester.Api.GetLibraryAsync(parameters);

			tester.CapturedRequest
				.RequestUri.OriginalString
				.Should().Be(expected);
		}
	}

	[TestClass]
	public class GetLibraryBookAsync
	{
		[TestMethod]
		public async Task null_asin_throws()
		{
			var tester = new ApiCallTester(LibraryFull);

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => tester.Api.GetLibraryBookAsync(null, null));
		}

		[TestMethod]
		public async Task blank_asin_throws()
		{
			var tester = new ApiCallTester(LibraryFull);

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => tester.Api.GetLibraryBookAsync("", LibraryOptions.ResponseGroupOptions.None));

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => tester.Api.GetLibraryBookAsync("   ", LibraryOptions.ResponseGroupOptions.None));
		}

		const string harryPotterAsin = "B017V4IM1G";
		static string LibBookDefault => $"/1.0/library/{harryPotterAsin}";

		[TestMethod]
		public async Task null_returns_default()
			=> await test(null, LibBookDefault);

		[TestMethod]
		public async Task None_returns_default()
			=> await test(LibraryOptions.ResponseGroupOptions.None, LibBookDefault);

		[TestMethod]
		public async Task empty_returns_default()
			=> await test("", LibBookDefault);

		[TestMethod]
		public async Task whitespace_returns_default()
			=> await test("   ", LibBookDefault);

		[TestMethod]
		public async Task question_returns_default()
			=> await test(" ? ", LibBookDefault);

		[TestMethod]
		public async Task strongly_typed_1()
			=> await test(LibraryOptions.ResponseGroupOptions.Sku, "/1.0/library/B017V4IM1G?response_groups=sku");

		[TestMethod]
		public async Task strongly_typed_multiple()
			=> await test(LibraryOptions.ResponseGroupOptions.Sku | LibraryOptions.ResponseGroupOptions.Series, "/1.0/library/B017V4IM1G?response_groups=series,sku");

		[TestMethod]
		public async Task accepts_question_prefix()
			=> await test("?response_groups=sku", "/1.0/library/B017V4IM1G?response_groups=sku");

		[TestMethod]
		public async Task accepts_no_question_prefix()
			=> await test("response_groups=sku", "/1.0/library/B017V4IM1G?response_groups=sku");

		[TestMethod]
		public async Task asin_with_whitespace()
			=> await test("   B017V4IM1G   ", null, LibBookDefault);

		[TestMethod]
		public async Task lowercase_asin()
			=> await test("b017v4im1g", null, LibBookDefault);

		private static Task test(LibraryOptions.ResponseGroupOptions responseGroups, string expected)
			=> test(harryPotterAsin, responseGroups, expected);

		private static Task test(string parameters, string expected)
			=> test(harryPotterAsin, parameters, expected);

		private static Task test(string asin, LibraryOptions.ResponseGroupOptions responseGroups, string expected)
			=> test(asin, responseGroups.ToQueryString(), expected);

		private static async Task test(string asin, string parameters, string expected)
		{
			var tester = new ApiCallTester(LibraryFull);
			await tester.Api.GetLibraryBookAsync(asin, parameters);

			tester.CapturedRequest
				.RequestUri.OriginalString
				.Should().Be(expected);
		}
	}
}
