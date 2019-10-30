using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Moq;
using TestCommon;

namespace TestAudibleApiCommon
{
	public static class ApiHttpClientMock
	{
		public static ApiHttpClient GetClient(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			var handler = GetHandler(handlerReturnString, statusCode);
			return ApiHttpClient.Create(handler);
		}

		public static ApiHttpClient GetClient(HttpResponseMessage response)
		{
			var handler = GetHandler(response);
			return ApiHttpClient.Create(handler);
		}

		public static HttpClientHandler GetHandler(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
			 => HttpMock.CreateMockHttpClientHandler
				(
				handlerReturnString ?? "foo",
				statusCode
				).Object;

		public static HttpClientHandler GetHandler(HttpResponseMessage response)
			=> HttpMock.CreateMockHttpClientHandler(response).Object;

		public static async Task<Api> GetApiAsync(string handlerReturnString)
		{
			var handler = GetHandler(handlerReturnString);
			return await GetApiAsync(handler);
		}

		public static async Task<Api> GetApiAsync(HttpClientHandler handler)
		{
			var idMgr = AuthorizationShared.Shared.GetIdentity_Future();
			var sharer = new ClientSharer(handler);
			var systemDateTime = StaticSystemDateTime.Past;
			var authorize = new Authorize(sharer, systemDateTime);
			var maintainer = await IdentityMaintainer.CreateAsync(idMgr, authorize, systemDateTime);
			return new Api(maintainer, sharer);
		}
	}
}
