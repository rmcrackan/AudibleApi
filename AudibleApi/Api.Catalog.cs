using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi.Common;

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

	public partial class Api
	{
		const string CATALOG_PATH = "/1.0/catalog";

		public async Task<Item> GetCatalogProductAsync(string asin, CatalogOptions.ResponseGroupOptions responseGroups)
			=> (await GetCatalogProductsAsync(new CatalogOptions { ResponseGroups = responseGroups, Asins = new() { asin } })).Single();

		public Task<List<Item>> GetCatalogProductsAsync(IEnumerable<string> asins, CatalogOptions.ResponseGroupOptions responseGroups)
			=> GetCatalogProductsAsync(new CatalogOptions { ResponseGroups = responseGroups, Asins = asins.ToList() });

		public async Task<List<Item>> GetCatalogProductsAsync(CatalogOptions catalogOptions)
		{
			if (catalogOptions is null)
				throw new ArgumentNullException(nameof(catalogOptions));

			var options = catalogOptions.ToQueryString();
			options = options?.Trim().Trim('?');

			var url = $"{CATALOG_PATH}/products/";
			if (!string.IsNullOrWhiteSpace(options))
				url += "?" + options;
			var obj = await AdHocNonAuthenticatedGetAsync(url);
			var objStr = obj.ToString();

			ProductsDtoV10 dto;
			try
			{
				// important! use this convert/deser method
				dto = ProductsDtoV10.FromJson(objStr);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error converting catalog product. Full json:\r\n" + objStr);
				throw;
			}

			return dto.Products.ToList();
		}
	}
}
