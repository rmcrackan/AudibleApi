using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Common
{
	// DO NOT combine with DtoBase
	// keeping Converter separate enables things like:
	//   var list = AudibleApi.Common.Converter.FromJson<List<Item>>(json);
	//   var list = AudibleApi.Common.Converter.FromJson<Item[]>(json);
	public static class Converter
	{
		private static JsonSerializerSettings ReadSettings { get; } = new()
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
				new RecordConverter(),
				new DtoConverter()
			},
			Error = (sender, args) =>
			{
				Serilog.Log.Logger.Error(args.ErrorContext.Error, $"Deserialization error|Message: {args?.ErrorContext?.Error?.Message}|args:{args}");
				args.ErrorContext.Handled = true;
			}
		};

		private static JsonSerializerSettings WriteSettings { get; } = new()
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
				new RecordConverter(),
			},
		};

		public static async Task<T> ReadAsDtoAsync<T>(this HttpContent content) where T : DtoBase<T>
		{
			var jobj = await content.ReadAsJObjectAsync();
			try
			{
				return DtoBase<T>.FromJson(jobj);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Error converting {typeof(T).Name}. Full json:\r\n" + jobj.ToString(Formatting.None));
				throw;
			}
		}

		/// <summary>json => object using AudibleApi.Common serializers</summary>
		public static T FromJson<T>(JObject json) => json.ToObject<T>(JsonSerializer.Create(ReadSettings));

		/// <summary>object => json using AudibleApi.Common serializers</summary>
		public static string ToJson(object self) => JsonConvert.SerializeObject(self, Formatting.Indented, WriteSettings);
	}
}
