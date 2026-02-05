using Newtonsoft.Json.Linq;
using System;

namespace AudibleApi;

public static class ExceptionExtensions
{
	public static JObject ToJson(this Exception ex, string message)
	{
		var json = new JObject
		{
			{ "error", message },
			{ "error_message", ex.Message },
		};
		if (ex is System.Net.Http.HttpRequestException httpEx)
		{
			json.Add("http_status_code", httpEx.StatusCode?.ToString());
			json.Add("http_request_error", httpEx.HttpRequestError.ToString());
		}
		json.Add("error_stack_trace", ex.StackTrace);

		// Handle inner exceptions
		var innerException = ex.InnerException;
		var count = 1;
		while (innerException != null)
		{
			var innerJson = new JObject
			{
				{ "error_message", innerException.Message },
				{ "error_stack_trace", innerException.StackTrace }
			};

			// Add inner exception details to the main json
			json.Add($"inner_exception_{count++}", innerJson);

			// Move to the next inner exception if available
			innerException = innerException.InnerException;
		}

		return json;
	}
}
