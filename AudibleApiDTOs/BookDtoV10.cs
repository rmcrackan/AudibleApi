using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

	public static partial class Serialize
	{
		public static string ToJson(this BookDtoV10 self) => JsonConvert.SerializeObject(self, AudibleApiDTOs.Converter.Settings);
	}
}
