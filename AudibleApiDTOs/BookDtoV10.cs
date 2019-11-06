using System;
using Newtonsoft.Json;

namespace AudibleApiDTOs
{
	public partial class BookDtoV10
	{
		[JsonProperty("item")]
		public Item Item { get; set; }

		[JsonProperty("response_groups")]
		public string[] ResponseGroups { get; set; }
	}

	public partial class BookDtoV10
	{
		public static BookDtoV10 FromJson(string json) => JsonConvert.DeserializeObject<BookDtoV10>(json, AudibleApiDTOs.Converter.Settings);
	}
}
