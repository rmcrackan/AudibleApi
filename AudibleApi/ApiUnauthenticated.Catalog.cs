using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi.Common;
using Dinah.Core;

namespace AudibleApi
{
	public class CatalogOptions
	{
		[Flags]
		public enum ResponseGroupOptions
		{
			None = 0,
			[Description("contributors")]
			Contributors = 1 << 0,
			[Description("media")]
			Media = 1 << 1,
			[Description("product_attrs")]
			ProductAttrs = 1 << 2,
			[Description("product_desc")]
			ProductDesc = 1 << 3,
			[Description("product_extended_attrs")]
			ProductExtendedAttrs = 1 << 4,
			[Description("product_plan_details")]
			ProductPlanDetails = 1 << 5,
			[Description("product_plans")]
			ProductPlans = 1 << 6,
			[Description("rating")]
			Rating = 1 << 7,
			[Description("review_attrs")]
			ReviewAttrs = 1 << 8,
			[Description("reviews")]
			Reviews = 1 << 9,
			[Description("sample")]
			Sample = 1 << 10,
			[Description("sku")]
			Sku = 1 << 11,
			[Description("relationships")]
			Relationships = 1 << 12,
			[Description("category_ladders")]
			CategoryLadders = 1 << 13,
			[Description("claim_code_url")]
			ClaimCodeUrl = 1 << 14,
			[Description("price")]
			Price = 1 << 15,
			[Description("provided_review")]
			ProvidedReview = 1 << 16,
			[Description("series")]
			Series = 1 << 17,
			ALL_OPTIONS = (1 << 18) - 1
		}
		public ResponseGroupOptions ResponseGroups { get; set; }

		public enum SortByOptions
		{
			None,
			[Description("-MostHelpful")]
			MostHelpfulDesc,
			[Description("-MostRecent")]
			MostRecentDesc,
			[Description("MostHelpful")]
			MostHelpful,
			[Description("MostRecent")]
			MostRecent
		}
		public SortByOptions SortBy { get; set; }

		public List<string> Asins { get; set; } = new List<string>();

		public string ToQueryString()
		{
			var parameters = new List<string>();

			if (ResponseGroups != ResponseGroupOptions.None)
				parameters.Add(ResponseGroups.ToResponseGroupsQueryString());

			if (SortBy != SortByOptions.None)
				parameters.Add(SortBy.ToSortByQueryString());

			if (Asins is not null)
			{
				var ids = Asins.Select(id => id.ToUpper().Trim()).Where(id => !string.IsNullOrWhiteSpace(id));
				if (ids.Any())
					parameters.Add("asins=" + ids.Aggregate((a, b) => $"{a},{b}"));
			}

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
	}

	public partial class ApiUnauthenticated
	{
		protected const string CATALOG_PATH = "/1.0/catalog";
		public const int MaxAsinsPerRequest = 50;


		public async Task<Item> GetCatalogProductAsync(string asin, CatalogOptions.ResponseGroupOptions responseGroups)
			=> (await GetCatalogProductsAsync(new[] { asin }, responseGroups)).Single();

		public async Task<List<Item>> GetCatalogProductsAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups)
		{
			var options = new CatalogOptions
			{
				ResponseGroups = responseGroups,
				Asins = asins.ToList()
			};

			var dto = await getCatalogPageAsync(null, options);
			return dto.Products.ToList();
		}

		public async Task<List<Item>> GetCatalogProductsAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphoreSlim = new SemaphoreSlim(maxConcurrentRequests);

			return await GetCatalogProductsAsync(asins, responseGroups, numItemsPerRequest, semaphoreSlim);
		}

		public async Task<List<Item>> GetCatalogProductsAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			var allItems = new List<Item>();

			await foreach (var items in GetCatalogPagesAsync(asins, responseGroups, numItemsPerRequest, semaphore))
				allItems.AddRange(items);

			return allItems;
		}

		public async IAsyncEnumerable<Item> GetCatalogProductsAsyncEnumerable(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			await foreach (var page in GetCatalogPagesAsync(asins, responseGroups, numItemsPerRequest, maxConcurrentRequests))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Item> GetCatalogProductsAsyncEnumerable(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			await foreach (var page in GetCatalogPagesAsync(asins, responseGroups, numItemsPerRequest, semaphore))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Item[]> GetCatalogPagesAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphore = new SemaphoreSlim(maxConcurrentRequests);

			await foreach (var item in GetCatalogPagesAsync(asins, responseGroups, numItemsPerRequest, semaphore))
				yield return item;
		}

		public async IAsyncEnumerable<Item[]> GetCatalogPagesAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			var asinList = ArgumentValidator.EnsureNotNull(asins, nameof(asins)).ToList();
			ArgumentValidator.EnsureGreaterThan(asinList.Count, nameof(asinList), 0);
			ArgumentValidator.EnsureBetweenInclusive(numItemsPerRequest, nameof(numItemsPerRequest), 1, MaxAsinsPerRequest);

			List<Task<ProductsDtoV10>> pageDlTasks = new();

			int page = 0;
			int totalItems = asins.Count();
			int totalPages = totalItems / numItemsPerRequest;
			if (totalPages * numItemsPerRequest < totalItems) totalPages++;

			//Spin up as many concurrent downloads as we can/need. Minimum 1.
			do
				await spinupPageRequest();
			while (semaphore.CurrentCount > 0 && page < totalPages);

			while (pageDlTasks.Count > 0)
			{
				var completed = await Task.WhenAny(pageDlTasks);
				pageDlTasks.Remove(completed);

				//Request new page(s).
				while (semaphore.CurrentCount > 0 && page < totalPages)
					await spinupPageRequest();

				yield return completed.Result.Products;
			}

			async Task spinupPageRequest()
			{
				var options = new CatalogOptions
				{
					ResponseGroups = responseGroups,
					Asins = asinList.Skip(page * numItemsPerRequest).Take(numItemsPerRequest).ToList()
				};
				await semaphore.WaitAsync();
				pageDlTasks.Add(getCatalogPageAsync(semaphore, options));
				page++;
			}
		}

		protected virtual async Task<ProductsDtoV10> getCatalogPageAsync(SemaphoreSlim semaphore, CatalogOptions catalogOptions)
		{
			try
			{
				ArgumentValidator.EnsureBetweenInclusive(catalogOptions.Asins.Count, $"{nameof(catalogOptions)}.Asins.Count", 1, MaxAsinsPerRequest);
				var options = ArgumentValidator.EnsureNotNull(catalogOptions, nameof(catalogOptions)).ToQueryString();
				options = options?.Trim().Trim('?');

				var url = $"{CATALOG_PATH}/products/";
				if (!string.IsNullOrWhiteSpace(options))
					url += "?" + options;
				var response = await AdHocNonAuthenticatedGetAsync(url);
				var dto = await response.Content.ReadAsDtoAsync<ProductsDtoV10>();

				return dto;
			}
			finally { semaphore?.Release(); }
		}
	}
}
