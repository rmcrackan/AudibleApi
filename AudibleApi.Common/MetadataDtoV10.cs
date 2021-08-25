using Newtonsoft.Json;
using System.Text;

namespace AudibleApi.Common
{
    public partial class MetadataDtoV10
    {
        [JsonProperty("content_metadata")]
        public ContentMetadata ContentMetadata { get; set; }

        [JsonProperty("response_groups")]
        public string[] ResponseGroups { get; set; }
    }

    public partial class ContentMetadata
    {
        [JsonProperty("chapter_info")]
        public ChapterInfo ChapterInfo { get; set; }

        [JsonProperty("content_reference")]
        public ContentReference ContentReference { get; set; }
    }

    public partial class ChapterInfo
    {
        [JsonProperty("brandIntroDurationMs")]
        public long BrandIntroDurationMs { get; set; }

        [JsonProperty("brandOutroDurationMs")]
        public long BrandOutroDurationMs { get; set; }

        [JsonProperty("chapters")]
        public Chapter[] Chapters { get; set; }

        [JsonProperty("is_accurate")]
        public bool IsAccurate { get; set; }

        [JsonProperty("runtime_length_ms")]
        public long RuntimeLengthMs { get; set; }

        [JsonProperty("runtime_length_sec")]
        public long RuntimeLengthSec { get; set; }
    }

    public partial class Chapter
    {
        [JsonProperty("length_ms")]
        public long LengthMs { get; set; }

        [JsonProperty("start_offset_ms")]
        public long StartOffsetMs { get; set; }

        [JsonProperty("start_offset_sec")]
        public long StartOffsetSec { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class ContentReference
    {
        [JsonProperty("acr")]
        public string Acr { get; set; }

        [JsonProperty("asin")]
        public string Asin { get; set; }

        [JsonProperty("content_format")]
        public string ContentFormat { get; set; }

        [JsonProperty("content_size_in_bytes")]
        public long ContentSizeInBytes { get; set; }

        [JsonProperty("file_version")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FileVersion { get; set; }

        [JsonProperty("marketplace")]
        public string Marketplace { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("tempo")]
        public string Tempo { get; set; }

        [JsonProperty("version")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Version { get; set; }
    }

    public partial class MetadataDtoV10
    {
        public static MetadataDtoV10 FromJson(string json) => JsonConvert.DeserializeObject<MetadataDtoV10>(json, AudibleApi.Common.Converter.Settings);
    }
    public static partial class Serialize
    {
        public static string ToJson(this MetadataDtoV10 self) => JsonConvert.SerializeObject(self, AudibleApi.Common.Converter.Settings);
    }
}
