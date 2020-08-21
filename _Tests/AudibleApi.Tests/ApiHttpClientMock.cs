using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Moq;
using TestCommon;
using static AuthorizationShared.Shared;
using static AuthorizationShared.Shared.AccessTokenTemporality;

namespace TestAudibleApiCommon
{
	public static class ApiHttpClientMock
	{
		public static ApiHttpClient GetClient(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			var handler = HttpMock.GetHandler(handlerReturnString, statusCode);
			return ApiHttpClient.Create(handler);
		}

		public static ApiHttpClient GetClient(HttpResponseMessage response)
		{
			var handler = HttpMock.GetHandler(response);
			return ApiHttpClient.Create(handler);
		}

		public static async Task<Api> GetApiAsync(string handlerReturnString)
		{
			var handler = HttpMock.GetHandler(handlerReturnString);
			return await GetApiAsync(handler);
		}

		public static async Task<Api> GetApiAsync(HttpClientHandler handler)
		{
			var idMgr = GetIdentity(Future);
			var sharer = new HttpClientSharer(handler);
			var systemDateTime = StaticSystemDateTime.Past;
			var authorize = new Authorize(Locale.Empty, sharer, systemDateTime);
			var maintainer = await IdentityMaintainer.CreateAsync(idMgr, authorize, systemDateTime);
			return new Api(maintainer, sharer);
		}
	}
}
