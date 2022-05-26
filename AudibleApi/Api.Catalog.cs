using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public static class CatalogQueryStringBuilderExtensions
	{
		public static string ToQueryString(this CatalogOptions.ResponseGroupOptions responseGroupOptions)
		{
			if (responseGroupOptions == CatalogOptions.ResponseGroupOptions.None)
				return "";

			var descriptions = responseGroupOptions
				.ToValues()
				.Select(e => e.GetDescription())
				.ToList();
			if (!descriptions.Any() || descriptions.Any(d => d is null))
				throw new Exception("Unexpected value in response group");
			var str = "response_groups=" + descriptions.Aggregate((a, b) => $"{a},{b}");
			return str;
		}

		public static string ToQueryString(this CatalogOptions.SortByOptions sortByOptions)
		{
			if (sortByOptions == CatalogOptions.SortByOptions.None)
				return "";

			var description = sortByOptions.GetDescription();
			if (description is null)
				throw new Exception("Unexpected value for sort by");
			var str = "sort_by=" + description;
			return str;
		}

		public static string ToQueryString(this CatalogOptions catalogOptions)
		{
			var parameters = new List<string>();

			if (catalogOptions.ResponseGroups != CatalogOptions.ResponseGroupOptions.None)
				parameters.Add(catalogOptions.ResponseGroups.ToQueryString());

			if (catalogOptions.SortBy != CatalogOptions.SortByOptions.None)
				parameters.Add(catalogOptions.SortBy.ToQueryString());

			if (catalogOptions.Asins is not null)
			{
				var ids = catalogOptions.Asins.Select(id => id.ToUpper().Trim()).Where(id => !string.IsNullOrWhiteSpace(id));
				if (ids.Any())
					parameters.Add("asins=" + ids.Aggregate((a, b) => $"{a},{b}"));
			}

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
	}
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
			// https://stackoverflow.com/questions/7467722
			ALL_OPTIONS = ~(1 << 12)
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
	}

	public partial class Api
	{
		const string CATALOG_PATH = "/1.0/catalog";

		#region GetCatalogProductAsync
		public Task<Item> GetCatalogProductAsync(string asin, CatalogOptions.ResponseGroupOptions responseGroups)
			=> GetCatalogProductAsync(asin, responseGroups.ToQueryString());

		public async Task<Item> GetCatalogProductAsync(string asin, string responseGroups)
		{
			if (asin is null)
				throw new ArgumentNullException(nameof(asin));
			if (string.IsNullOrWhiteSpace(asin))
				throw new ArgumentException("asin may not be blank", nameof(asin));

			asin = asin.ToUpper().Trim();

			responseGroups = responseGroups?.Trim().Trim('?');

			var url = $"{CATALOG_PATH}/products/{asin}";
			if (!string.IsNullOrWhiteSpace(responseGroups))
				url += "?" + responseGroups;
			var response = await AdHocAuthenticatedGetAsync(url);
			var obj = await response.Content.ReadAsJObjectAsync();
			var objStr = obj.ToString();

			ProductDtoV10 dto;
			try
			{
				// important! use this convert/deser method
				dto = ProductDtoV10.FromJson(objStr);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error converting catalog product. Full json:\r\n" + objStr);
				throw;
			}

			return dto.Product;
		}
		#endregion

		#region GetCatalogProductsAsync
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
			var response = await AdHocAuthenticatedGetAsync(url);
			var obj = await response.Content.ReadAsJObjectAsync();
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
		#endregion
	}
}
