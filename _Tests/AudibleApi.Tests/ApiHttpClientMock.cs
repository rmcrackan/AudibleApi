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
