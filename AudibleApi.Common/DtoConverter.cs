using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AudibleApi.Common
{
	internal class DtoConverter : JsonConverter
	{
		private bool skipNextMatch = false;
		public override bool CanConvert(Type objectType)
		{
			if (objectType.IsAssignableTo(typeof(DtoBase)))
			{
				if (skipNextMatch)
				{
					skipNextMatch = false;
					return false;
				}
				return true;
			}
			return false;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var dtoJObject = JObject.Load(reader);

			//We've already matched to the current dto type. Set skipNextMatch to true
			//so that when ToObject() is called, this instance of JsonConverter will
			//tell the serializer that it is not a match for the current data type and
			//the serializer will move on. After a match is skipped, the skipNextMatch
			//flag is reset to false so that any children members that are DtoBase will
			//again be passed through this converter.
			skipNextMatch = true;
			var dto = dtoJObject.ToObject(objectType, serializer) as DtoBase;
			dto.SourceJson = dtoJObject;

			return dto;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			=> throw new NotImplementedException();
	}
}
