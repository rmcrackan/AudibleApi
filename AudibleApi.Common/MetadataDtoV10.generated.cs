using Newtonsoft.Json;
using System.Text;

#nullable enable
namespace AudibleApi.Common;

public partial class MetadataDtoV10
{
	[JsonProperty("content_metadata")]
	public required ContentMetadata ContentMetadata { get; set; }
}

public partial class ContentMetadata
{
	[JsonProperty("chapter_info")]
	public required ChapterInfo ChapterInfo { get; set; }

	[JsonProperty("content_reference")]
	public required ContentReference ContentReference { get; set; }
}

public partial class ChapterInfo
{
	[JsonProperty("brandIntroDurationMs")]
	public long BrandIntroDurationMs { get; set; }

	[JsonProperty("brandOutroDurationMs")]
	public long BrandOutroDurationMs { get; set; }

	[JsonProperty("chapters")]
	public Chapter[]? Chapters { get; set; }

	[JsonProperty("is_accurate")]
	public bool IsAccurate { get; set; }

	[JsonProperty("runtime_length_ms")]
	public long RuntimeLengthMs { get; set; }

	[JsonProperty("runtime_length_sec")]
	public long RuntimeLengthSec { get; set; }
}

public partial class Chapter
{
	[JsonProperty("chapters")]
	public Chapter[]? Chapters { get; set; }

	[JsonProperty("length_ms")]
	public long LengthMs { get; set; }

	[JsonProperty("start_offset_ms")]
	public long StartOffsetMs { get; set; }

	[JsonProperty("start_offset_sec")]
	public long StartOffsetSec { get; set; }

	[JsonProperty("title")]
	public required string Title { get; set; }
	public override string? ToString() => Title;
}

public record ContentReference
{
	[JsonProperty("acr")]
	public required string Acr { get; set; }

	[JsonProperty("asin")]
	public required string Asin { get; set; }

	[JsonProperty("codec", Required = Required.DisallowNull)]
	public required string Codec { get; set; }

	[JsonProperty("content_format")]
	public required string ContentFormat { get; set; }

	[JsonProperty("content_size_in_bytes")]
	public long ContentSizeInBytes { get; set; }

	[JsonProperty("file_version")]
	[JsonConverter(typeof(ParseStringConverter))]
	public long FileVersion { get; set; }

	[JsonProperty("marketplace")]
	public required string Marketplace { get; set; }

	[JsonProperty("sku")]
	public required string Sku { get; set; }

	[JsonProperty("tempo")]
	public required string Tempo { get; set; }

	[JsonProperty("version")]
	public required string Version { get; set; }

	public bool IsSpatial => Codec is "ec+3" or "ac-4";
}
