using Newtonsoft.Json;

namespace AudibleApi.Common;

/// <summary>
/// Product (aka: catalog product) and Book.Item (from user's library) are the same except that the library book has much more information
/// </summary>
public partial class ProductsDtoV10 : V10Base<ProductsDtoV10>
{
	[JsonProperty("products")]
	public Item[]? Products { get; set; }
}
