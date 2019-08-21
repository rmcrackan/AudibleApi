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
	public static class ApiClientMock
	{
		public static ApiClient GetClient(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			var handler = GetHandler(handlerReturnString, statusCode);
			return ApiClient.Create(handler);
		}

		public static ApiClient GetClient(HttpResponseMessage response)
		{
			var handler = GetHandler(response);
			return ApiClient.Create(handler);
		}

		public static HttpClientHandler GetHandler(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
			 => HttpMock.CreateMockHttpClientHandler
				(
				handlerReturnString ?? "foo",
				statusCode
				).Object;

		public static HttpClientHandler GetHandler(HttpResponseMessage response)
			=> HttpMock.CreateMockHttpClientHandler(response).Object;

		public static async Task<Api> GetApi(string handlerReturnString)
		{
			var handler = GetHandler(handlerReturnString);
			return await GetApi(handler);
		}

		public static async Task<Api> GetApi(HttpClientHandler handler)
		{
			var idMgr = AuthorizationShared.Shared.GetIdentity();
			var sharer = new ClientSharer(handler);
			var systemDateTime = StaticSystemDateTime.ByYear(2000);
			var authorize = new Authorize(sharer, systemDateTime);
			var maintainer = await IdentityMaintainer.CreateAsync(idMgr, authorize, systemDateTime);
			return new Api(maintainer, sharer);
		}
	}
}
