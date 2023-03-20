using Newtonsoft.Json;
using System;

namespace AudibleApi.Common
{
	public partial class WishListDtoV10
	{
		[JsonProperty("bin_fields")]
		public object BinFields { get; set; }

		[JsonProperty("products")]
		public Product[] Products { get; set; }

		[JsonProperty("total_results")]
		public int TotalResults { get; set; }
	}

	public partial class Product : DtoBase<Product>
	{
		[JsonProperty("added_timestamp")]
		public DateTimeOffset AddedTimestamp { get; set; }

		[JsonProperty("asin")]
		public string Asin { get; set; }

		[JsonProperty("audible_editors_summary")]
		public string AudibleEditorsSummary { get; set; }

		[JsonProperty("authors")]
		public Author[] Authors { get; set; }

		[JsonProperty("available_codecs")]
		public AvailableCodec[] AvailableCodecs { get; set; }

		[JsonProperty("content_delivery_type")]
		public string ContentDeliveryType { get; set; }

		[JsonProperty("content_type")]
		public string ContentType { get; set; }

		[JsonProperty("customer_rights")]
		public CustomerRights CustomerRights { get; set; }

		[JsonProperty("distribution_rights_region")]
		public string[] DistributionRightsRegion { get; set; }

		[JsonProperty("editorial_reviews")]
		public string[] EditorialReviews { get; set; }

		[JsonProperty("format_type")]
		public string FormatType { get; set; }

		[JsonProperty("has_children")]
		public bool? HasChildren { get; set; }

		[JsonProperty("is_adult_product")]
		public bool? IsAdultProduct { get; set; }

		[JsonProperty("is_buyable")]
		public bool? IsBuyable { get; set; }

		[JsonProperty("is_listenable")]
		public bool? IsListenable { get; set; }

		[JsonProperty("is_preorderable")]
		public bool? IsPreorderable { get; set; }

		[JsonProperty("is_purchasability_suppressed")]
		public bool? IsPurchasabilitySuppressed { get; set; }

		[JsonProperty("is_world_rights")]
		public bool? IsWorldRights { get; set; }

		[JsonProperty("issue_date")]
		public DateTimeOffset? IssueDate { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("merchandising_summary")]
		public string MerchandisingSummary { get; set; }

		[JsonProperty("narrators")]
		public Author[] Narrators { get; set; }

		[JsonProperty("preorder_release_date")]
		public DateTimeOffset? PreorderReleaseDate { get; set; }

		[JsonProperty("price")]
		public Price Price { get; set; }

		[JsonProperty("product_images")]
		public ProductImages ProductImages { get; set; }

		[JsonProperty("publication_datetime")]
		public DateTimeOffset? PublicationDatetime { get; set; }

		[JsonProperty("publication_name")]
		public string PublicationName { get; set; }

		[JsonProperty("publisher_name")]
		public string PublisherName { get; set; }

		[JsonProperty("publisher_summary")]
		public string PublisherSummary { get; set; }

		[JsonProperty("rating")]
		public Rating Rating { get; set; }

		[JsonProperty("relationships")]
		public Relationship[] Relationships { get; set; }

		[JsonProperty("release_date")]
		public DateTimeOffset? ReleaseDate { get; set; }

		[JsonProperty("runtime_length_min")]
		public long? RuntimeLengthMin { get; set; }

		[JsonProperty("sample_url")]
		public Uri SampleUrl { get; set; }

		[JsonProperty("sku")]
		public string Sku { get; set; }

		[JsonProperty("sku_lite")]
		public string SkuLite { get; set; }

		[JsonProperty("social_media_images")]
		public SocialMediaImages SocialMediaImages { get; set; }

		[JsonProperty("subtitle")]
		public string Subtitle { get; set; }

		[JsonProperty("thesaurus_subject_keywords")]
		public string[] ThesaurusSubjectKeywords { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }
	}

	public partial class Author
	{
		[JsonProperty("asin")]
		public string Asin { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public partial class CustomerRights
	{
		[JsonProperty("is_consumable")]
		public bool IsConsumable { get; set; }

		[JsonProperty("is_consumable_indefinitely")]
		public bool IsConsumableIndefinitely { get; set; }

		[JsonProperty("is_consumable_offline")]
		public bool IsConsumableOffline { get; set; }

		[JsonProperty("is_consumable_until")]
		public object IsConsumableUntil { get; set; }
	}

	public partial class SocialMediaImages
	{
		[JsonProperty("facebook")]
		public Uri Facebook { get; set; }

		[JsonProperty("twitter")]
		public Uri Twitter { get; set; }
	}
}
