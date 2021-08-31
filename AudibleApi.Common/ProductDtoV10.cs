using System;
using Newtonsoft.Json;

namespace AudibleApi.Common
{
	/// <summary>
	/// Product (aka: catalog product) and Book.Item (from user's library) are the same except that the library book has much more information
	/// </summary>
	public partial class ProductDtoV10
	{
		[JsonProperty("product")]
		public Item Product { get; set; }

		[JsonProperty("response_groups")]
		public string[] ResponseGroups { get; set; }
	}

	public partial class ProductDtoV10
	{
		public string ToJson() => JsonConvert.SerializeObject(this, Converter.Settings);
		public static ProductDtoV10 FromJson(string json) => JsonConvert.DeserializeObject<ProductDtoV10>(json, AudibleApi.Common.Converter.Settings);
	}
}
