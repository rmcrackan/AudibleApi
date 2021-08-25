using System;
using Newtonsoft.Json;

namespace AudibleApi.Common
{
	public static partial class Serialize
	{
		public static string ToJson(this BookDtoV10 self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
