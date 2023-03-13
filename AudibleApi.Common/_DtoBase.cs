using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace AudibleApi.Common
{
	public abstract class DtoBase
	{
		public JObject SourceJson { get; set; }
	}

	public abstract class DtoBase<T> : DtoBase where T : DtoBase<T>
	{
		public static T FromJson(string json) => FromJson(JObject.Parse(json));
		public static T FromJson(JObject json) => Converter.FromJson<T>(json);
		public string ToJson(T dto) => Converter.ToJson(dto);
	}
}
