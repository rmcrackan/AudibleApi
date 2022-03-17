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
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.PageNumber = -1);
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.PageNumber = 0);
			Assert.ThrowsException<ArgumentException>(() => libraryOptions.PageNumber = 41);
		}

		[TestMethod]
		public void in_range()
		{
			var libraryOptions = new LibraryOptions();

			libraryOptions.PageNumber = null;
			libraryOptions.PageNumber.Should().BeNull();

			libraryOptions.PageNumber = 1;
			libraryOptions.PageNumber.Should().Be(1);

			libraryOptions.PageNumber = 40;
			libraryOptions.PageNumber.Should().Be(40);
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
