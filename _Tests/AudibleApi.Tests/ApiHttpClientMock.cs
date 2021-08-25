using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Moq;
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
	}
}
