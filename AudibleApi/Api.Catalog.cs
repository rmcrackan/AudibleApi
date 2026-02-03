using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AudibleApi;

public partial class Api
{
	public Task<bool> ReviewOverallAsync(string asin, int overallRating)
		=> ReviewAsync(asin, overallRating, 0, 0);
	public Task<bool> ReviewPerformanceAsync(string asin, int performanceRating)
		=> ReviewAsync(asin, 0, performanceRating, 0);
	public Task<bool> ReviewStoryAsync(string asin, int storyRating)
		=> ReviewAsync(asin, 0, 0, storyRating);

	public async Task<bool> ReviewAsync(string asin, int overall, int performance, int story)
	{
		const string requestUri = $"{CATALOG_PATH}/review";

		ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
		ArgumentValidator.EnsureBetweenInclusive(overall, nameof(overall), 0, 5);
		ArgumentValidator.EnsureBetweenInclusive(performance, nameof(performance), 0, 5);
		ArgumentValidator.EnsureBetweenInclusive(story, nameof(story), 0, 5);

		List<JObject> ratings = new();

		if (overall != 0)
			ratings.Add(new JObject { { "dimension", "overall" }, { "rating", overall } });
		if (performance != 0)
			ratings.Add(new JObject { { "dimension", "performance" }, { "rating", performance } });
		if (story != 0)
			ratings.Add(new JObject { { "dimension", "story" }, { "rating", story } });

		if (ratings.Count == 0)
			throw new ArgumentException($"At least one of {nameof(overall)}, {nameof(performance)}, or {nameof(story)} must be nonzero");

		var body = new JObject()
		{
			{ "review",  new JObject
				{
					{ "asin", asin },
					{ "rating_dimensions", JArray.FromObject(ratings) }
				}
			}
		};

		try
		{
			var response = await AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Put, Client, body);

			var responseJobj = await response.Content.ReadAsJObjectAsync();

			if (string.IsNullOrEmpty(responseJobj.Value<string>("review_id")))
			{
				Serilog.Log.Error("User review was not added {@DebugInfo}", responseJobj.ToString(Newtonsoft.Json.Formatting.None));
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "Error encountered while trying to set user rating.");
			return false;
		}
	}

	protected override async Task<ProductsDtoV10> getCatalogPageAsync(SemaphoreSlim? semaphore, CatalogOptions catalogOptions)
	{
		try
		{
			ArgumentValidator.EnsureBetweenInclusive(catalogOptions.Asins.Count, $"{nameof(catalogOptions)}.Asins.Count", 1, MaxAsinsPerRequest);
			var options = ArgumentValidator.EnsureNotNull(catalogOptions, nameof(catalogOptions)).ToQueryString();
			options = options?.Trim().Trim('?');

			var url = $"{CATALOG_PATH}/products/";
			if (!string.IsNullOrWhiteSpace(options))
				url += "?" + options;
			var response = await AdHocAuthenticatedGetAsync(url);
			var dto = await response.Content.ReadAsDtoAsync<ProductsDtoV10>();

			return dto;
		}
		finally { semaphore?.Release(); }
	}
}
