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
	public class IsInWishListAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.IsInWishListAsync(null));
		}

		[TestMethod]
		public async Task empty_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.IsInWishListAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.IsInWishListAsync("   "));
		}
	}

	[TestClass]
	public class AddToWishListAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.AddToWishListAsync(null));
		}

		[TestMethod]
		public async Task empty_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AddToWishListAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.AddToWishListAsync("   "));
		}
	}

	[TestClass]
	public class DeleteFromWishListAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task null_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => api.DeleteFromWishListAsync(null));
		}

		[TestMethod]
		public async Task empty_param_throws()
		{
			api ??= await ApiClientMock.GetApiAsync("x");

			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DeleteFromWishListAsync(""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => api.DeleteFromWishListAsync("   "));
		}
	}
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
	[TestClass]
	public class IsInWishListAsync
	{
		[TestMethod]
		public async Task pagination_called_3x()
		{
			var response = new HttpResponseMessage
			{
				Content = new StringContent(@"
                    {
                      ""total_results"": 5,
                      ""products"": [
                        { ""asin"": ""1984885170"" },
                        { ""asin"": ""B077H4K2HM"" }
                      ]
                    }
                ")
			};

			var mock = HttpMock.CreateMockHttpClientHandler(response);

			var api = await ApiClientMock.GetApiAsync(mock.Object);

			var result = await api.IsInWishListAsync("172137406X");
			result.Should().BeFalse();
			mock.Protected().Verify(
				  "SendAsync",
				  Times.Exactly(3),
				  ItExpr.IsAny<HttpRequestMessage>(),
				  ItExpr.IsAny<CancellationToken>());
		}

		[TestMethod]
		public async Task not_in_wishlist()
		{
			var response = new HttpResponseMessage
			{
				Content = new StringContent(@"
                    {
                      ""total_results"": 2,
                      ""products"": [
                        { ""asin"": ""1984885170"" },
                        { ""asin"": ""B077H4K2HM"" }
                      ]
                    }
                ")
			};

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var result = await api.IsInWishListAsync("172137406X");
			result.Should().BeFalse();
		}

		[TestMethod]
		public async Task in_wishlist()
		{
			var response = new HttpResponseMessage
			{
				Content = new StringContent(@"
                    {
                      ""total_results"": 2,
                      ""products"": [
                        { ""asin"": ""172137406X"" },
                        { ""asin"": ""B077H4K2HM"" }
                      ]
                    }
                ")
			};

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var result = await api.IsInWishListAsync("172137406X");
			result.Should().BeTrue();
		}
	}

	[TestClass]
	public class AddToWishListAsync
	{
		[TestMethod]
		public async Task bad_code()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("{}")
			};

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var ex = await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.AddToWishListAsync("172137406X"));
			ex.Message.Should().Be($"Add to Wish List failed. Invalid status code. Code: {HttpStatusCode.BadRequest}");
		}

		[TestMethod]
		public async Task bad_location()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Created,
				Content = new StringContent("{}")
			};
			response.Headers.Location = new Uri("/foo", UriKind.Relative);

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var ex = await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.AddToWishListAsync("172137406X"));
			ex.Message.Should().Be("Add to Wish List failed. Bad location. Location: /foo");
		}

		[TestMethod]
		public async Task success()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Created,
				Content = new StringContent("{}")
			};
			response.Headers.Location = new Uri("/1.0/wishlist/172137406X", UriKind.Relative);

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			await api.AddToWishListAsync("172137406X");
		}
	}

	[TestClass]
	public class DeleteFromWishListAsync
	{
		[TestMethod]
		public async Task bad_code()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.BadRequest,
				Content = new StringContent("{}")
			};

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			var ex = await Assert.ThrowsExceptionAsync<ApiErrorException>(() => api.DeleteFromWishListAsync("172137406X"));
			ex.Message.Should().Be($"Delete from Wish List failed. Invalid status code. Code: {HttpStatusCode.BadRequest}. Asin: 172137406X");
		}

		[TestMethod]
		public async Task success()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.NoContent,
				Content = new StringContent("{}")
			};

			var api = await ApiClientMock.GetApiAsync(HttpMock.CreateMockHttpClientHandler(response).Object);

			await api.DeleteFromWishListAsync("172137406X");
		}
	}
}
