using System;
using Newtonsoft.Json;

namespace AudibleApiDTOs
{
	public static partial class Serialize
	{
		public static string ToJson(this BookDtoV10 self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
