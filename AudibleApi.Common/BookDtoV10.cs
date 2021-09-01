using System;
using Newtonsoft.Json;

namespace AudibleApi.Common
{
	/// <summary>
	/// Product (aka: catalog product) and Book.Item (from user's library) are the same except that the library book has much more information
	/// </summary>
	public partial class BookDtoV10
	{
		[JsonProperty("item")]
		public Item Item { get; set; }

		[JsonProperty("response_groups")]
		public string[] ResponseGroups { get; set; }
	}

	public partial class BookDtoV10
	{
		public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented, Converter.Settings);
		public static BookDtoV10 FromJson(string json) => JsonConvert.DeserializeObject<BookDtoV10>(json, AudibleApi.Common.Converter.Settings);
	}
}
