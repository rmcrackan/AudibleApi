using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi.Common
{
	public partial class RecordDto : DtoBase<RecordDto>
	{
		[JsonProperty("md5")]
		public string Md5 { get; set; }

		[JsonProperty("payload")]
		public Payload Payload { get; set; }
	}

	public partial class Payload
	{
		[JsonProperty("acr")]
		public string Acr { get; set; }

		[JsonProperty("records")]
		[JsonConverter(typeof(RecordConverter))]
		public List<IRecord> Records { get; set; }

		[JsonProperty("guid")]
		public string Guid { get; set; }

		[JsonProperty("key")]
		public string Asin { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}

	internal class RecordConverter : JsonConverter
	{
		public override bool CanConvert(Type typeToConvert) => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			static IRecord createRecord(JObject jObj)
			{
				var type = jObj.Value<string>("type");
				var creationTime = DateTimeOffset.Parse(jObj.Value<string>("creationTime") + " Z").ToLocalTime();
				var startPosition = jObj.Value<long>("startPosition");

				if (type == RecordType.LastHeard)
					return new LastHeard(creationTime, startPosition);

				var lastModificationTime = DateTimeOffset.Parse(jObj.Value<string>("lastModificationTime") + " Z").ToLocalTime(); 
				var annotationId = jObj.Value<string>("annotationId");

				if (type == RecordType.Bookmark)
					return new Bookmark(creationTime, startPosition, annotationId, lastModificationTime);

				var endPosition = jObj.Value<long>("endPosition");

				if (type == RecordType.Note)
				{
					var text = jObj.Value<string>("text");
					return new Note(creationTime, startPosition, annotationId, lastModificationTime, endPosition, text);
				}

				if (type == RecordType.Clip)
				{
					var metadata = jObj.GetValue("metadata") as JObject;
					var text = metadata.Value<string>("note");
					var title = metadata.Value<string>("title");
					return new Clip(creationTime, startPosition, annotationId, lastModificationTime, endPosition, text, title);
				}

				Serilog.Log.Information($"Unknown Record type: {type}", jObj);
				return null;
			}

			return JArray.Load(reader)
				.Select(r => createRecord((JObject)r))
				.Where(r => r is not null)
				.ToList();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			=> throw new InvalidOperationException();
	}

	public interface IRecord
	{
		DateTimeOffset Created { get; }
		long StartPosition { get; }
		string GetName();
	}

	public interface IAnnotation : IRecord
	{
		DateTimeOffset LastModified { get; }
		string AnnotationId { get; }
	}

	public interface IRangeAnnotation : IAnnotation
	{
		long EndPosition { get; }
		string Text { get; }
	}

	public record LastHeard(DateTimeOffset Created, long StartPosition) : IRecord { public const string Name = "last_heard"; public string GetName() => Name; }
	public record Bookmark(DateTimeOffset Created, long StartPosition, string AnnotationId, DateTimeOffset LastModified) : IAnnotation { public const string Name = "bookmark"; public string GetName() => Name; }
	public record Note(DateTimeOffset Created, long StartPosition, string AnnotationId, DateTimeOffset LastModified, long EndPosition, string Text) : IRangeAnnotation { public const string Name = "note"; public string GetName() => Name; }
	public record Clip(DateTimeOffset Created, long StartPosition, string AnnotationId, DateTimeOffset LastModified, long EndPosition, string Text, string Title) : IRangeAnnotation { public const string Name = "clip"; public string GetName() => Name; }
}
