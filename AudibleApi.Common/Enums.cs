using System;

namespace AudibleApi.Common
{
	#region Item
	/// <summary>
	/// Item
	///		[JsonProperty("content_type")]
	///		public ContentType? ContentType { get; set; }
	/// </summary>
	public static class ContentType
	{
		public const string Product = "Product";
		public const string Episode = "Episode";
		public const string Lecture = "Lecture";
		public const string NewspaperMagazine = "Newspaper / Magazine";
		public const string Meditation = "Meditation";
		public const string Misc = "Misc";
		public const string Performance = "Performance";
		public const string RadioTvProgram = "Radio/TV Program";
		public const string Show = "Show";
		public const string Speech = "Speech";
		public const string Podcast = "Podcast";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("content_delivery_type")]
	///		public ContentDeliveryType? ContentDeliveryType { get; set; }
	/// </summary>
	public static class ContentDeliveryType
	{
		public const string SinglePartBook = "SinglePartBook";
		public const string MultiPartBook = "MultiPartBook";
		public const string SinglePartIssue = "SinglePartIssue";
		public const string MultiPartIssue = "MultiPartIssue";
		public const string Periodical = "Periodical";
		public const string PodcastParent = "PodcastParent";
		public const string PodcastEpisode = "PodcastEpisode";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("format_type")]
	///		public FormatType? FormatType { get; set; }
	/// </summary>
	public static class FormatType
	{
		public const string Abridged = "abridged";
		public const string OriginalRecording = "original_recording";
		public const string Unabridged = "unabridged";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("language")]
	///		public Language? Language { get; set; }
	/// </summary>
	public static class Language
	{
		public const string English = "english";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("origin_marketplace")]
	///		public OriginMarketplace? OriginMarketplace { get; set; }
	/// </summary>
	public static class OriginMarketplace
	{
		public const string AF2M0KC94RCEA = "AF2M0KC94RCEA";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("origin_type")]
	///		public OriginType? OriginType { get; set; }
	/// </summary>
	public static class OriginType
	{
		public const string AudibleChannels = "AudibleChannels";
		public const string AudibleComplimentaryOriginal = "AudibleComplimentaryOriginal";
		public const string Purchase = "Purchase";
		public const string Subscription = "Subscription";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("status")]
	///		public Status Status { get; set; }
	/// </summary>
	public static class Status
	{
		public const string Active = "Active";
	}

	/// <summary>
	/// Item
	///		[JsonProperty("thesaurus_subject_keywords")]
	///		public ThesaurusSubjectKeywords[] ThesaurusSubjectKeywords { get; set; }
	/// </summary>
	public static class ThesaurusSubjectKeyword
	{
		public const string AdventurersExplorers = "adventurers_&_explorers";
		public const string AlternateHistory = "alternate_history";
		public const string Comedians = "comedians";
		public const string Contemporary = "contemporary";
		public const string Dramatizations = "dramatizations";
		public const string EasternReligions = "eastern_religions";
		public const string LaConfidential = "la_confidential";
		public const string LiteratureAndFiction = "literature-and-fiction";
		public const string Medicine = "medicine";
		public const string Spirituality = "spirituality";
		public const string StandupComedy = "standup_comedy";
		public const string Storytelling = "storytelling";
		public const string SwordSorcery = "sword_&_sorcery";
		public const string Workouts = "workouts";
		public const string Terrorism = "terrorism";
	}
	#endregion

	#region AvailableCodec
	/// <summary>
	/// AvailableCodec
	///		[JsonProperty("enhanced_codec")]
	///		public EnhancedCodec? EnhancedCodec { get; set; }
	/// </summary>
	public static class EnhancedCodec
	{
		public const string Lc128_44100_Stereo = "LC_128_44100_stereo";
		public const string Lc32_22050_Stereo = "LC_32_22050_stereo";
		public const string Lc64_22050_Stereo = "LC_64_22050_stereo";
		public const string Lc64_44100_Stereo = "LC_64_44100_stereo";
		public const string Aax = "aax";
		public const string Format4 = "format4";
		public const string Mp42232 = "mp42232";
		public const string Mp42264 = "mp42264";
		public const string Mp444128 = "mp444128";
		public const string Mp44464 = "mp44464";
		public const string Piff2232 = "piff2232";
		public const string Piff2264 = "piff2264";
		public const string Piff44128 = "piff44128";
		public const string Piff4464 = "piff4464";
	}

	/// <summary>
	/// AvailableCodec
	///		[JsonProperty("format")]
	///		public AvailableCodecFormat? Format { get; set; }
	/// </summary>
	public static class AvailableCodecFormat
	{
		public const string Enhanced = "Enhanced";
		public const string Format4 = "Format4";
	}

	/// <summary>
	/// AvailableCodec
	///		[JsonProperty("name")]
	///		public Name? Name { get; set; }
	/// </summary>
	public static class Name
	{
		public const string Aax = "aax";
		public const string Aax22_32 = "aax_22_32";
		public const string Aax22_64 = "aax_22_64";
		public const string Aax44_128 = "aax_44_128";
		public const string Aax44_64 = "aax_44_64";
		public const string Format4 = "format4";
		public const string Mp422_32 = "mp4_22_32";
		public const string Mp422_64 = "mp4_22_64";
		public const string Mp444_128 = "mp4_44_128";
		public const string Mp444_64 = "mp4_44_64";
		public const string Piff22_32 = "piff_22_32";
		public const string Piff22_64 = "piff_22_64";
		public const string Piff44_128 = "piff_44_128";
		public const string Piff44_64 = "piff_44_64";
	}
	#endregion

	#region CategoryLadder
	/// <summary>
	/// CategoryLadder
	///		[JsonProperty("root")]
	///		public Root? Root { get; set; }
	/// </summary>
	public static class Root
	{
		public const string EditorsPicks = "EditorsPicks";
		public const string ExploreBy = "ExploreBy";
		public const string Genres = "Genres";
		public const string InstitutionsHpMarketing = "InstitutionsHpMarketing";
		public const string RodizioBuckets = "RodizioBuckets";
		public const string RodizioGenres = "RodizioGenres";
		public const string ShortsPrime = "ShortsPrime";
		public const string ShortsCurated = "ShortsCurated";
		public const string ShortsSandbox = "ShortsSandbox";
	}
	#endregion

	#region Review
	/// <summary>
	/// Review
	///		[JsonProperty("format")]
	///		public ReviewFormat? Format { get; set; }
	/// </summary>
	public static class ReviewFormat
	{
		public const string Freeform = "Freeform";
		public const string Guided = "Guided";
	}
	#endregion

	#region GuidedResponse
	/// <summary>
	/// GuidedResponse
	///		[JsonProperty("question_type")]
	///		public QuestionType? QuestionType { get; set; }
	/// </summary>
	public static class QuestionType
	{
		public const string Genre = "Genre";
		public const string Misc = "Misc";
		public const string Overall = "Overall";
		public const string Performance = "Performance";
		public const string Story = "Story";
	}
	#endregion

	#region Plan
	/// <summary>
	/// Plan
	///		[JsonProperty("plan_name")]
	///		public PlanName? PlanName { get; set; }
	/// </summary>
	public static class PlanName
	{
		public const string AyceRomance = "AyceRomance";
		public const string ComplimentaryOriginalMemberBenefit = "ComplimentaryOriginalMemberBenefit";
		public const string Radio = "Radio";
		public const string Rodizio = "Rodizio";
		public const string SpecialBenefit = "SpecialBenefit";
	}
	#endregion

	#region ListPriceClass
	/// <summary>
	/// ListPriceClass
	///		[JsonProperty("currency_code")]
	///		public CurrencyCode? CurrencyCode { get; set; }
	/// </summary>
	public static class CurrencyCode
	{
		public const string USD = "USD";
	}

	/// <summary>
	/// ListPriceClass
	///		[JsonProperty("merchant_id")]
	///		public MerchantId? MerchantId { get; set; }
	/// </summary>
	public static class MerchantId
	{
		public const string A2ZO8JX97D5MN9 = "A2ZO8JX97D5MN9";
	}

	/// <summary>
	/// ListPriceClass
	///		[JsonProperty("type")]
	///		public TypeEnum? Type { get; set; }
	/// </summary>
	public static class TypeEnum
	{
		public const string List = "list";
		public const string Member = "member";
		public const string Sale = "sale";
		public const string Ws4VUpsell = "ws4v_upsell";
	}
	#endregion

	#region Relationship
	/// <summary>
	/// Relationship
	///		[JsonProperty("relationship_to_product")]
	///		public RelationshipToProduct? RelationshipToProduct { get; set; }
	/// </summary>
	public static class RelationshipToProduct
	{
		public const string Child = "child";
		public const string Parent = "parent";
	}

	/// <summary>
	/// Relationship
	///		[JsonProperty("relationship_type")]
	///		public RelationshipType? RelationshipType { get; set; }
	/// </summary>
	public static class RelationshipType
	{
		public const string Component = "component";
		public const string Episode = "episode";
		public const string MerchantTitleAuthority = "merchant_title_authority";
		public const string Season = "season";
		public const string Series = "series";
	}
	#endregion

	#region License Rejection

	/// <summary>
	/// RejectionReason
	///		[JsonProperty("rejectionReason")]
	///		public string RejectionReason { get; set; }
	/// </summary>

	public static class RejectionReason
	{
		public const string ContentEligibility = "ContentEligibility";
		public const string RequesterEligibility = "RequesterEligibility";
		public const string GenericError = "GenericError";
	}

	/// <summary>
	/// ValidationType
	///		[JsonProperty("validationType")]
	///		public string ValidationType { get; set; }
	/// </summary>
	public static class ValidationType
	{
		public const string Client = "Client";
		public const string Ownership = "Ownership";
		public const string Membership = "Membership";
		public const string AYCL = "AYCL";
	}

	#endregion

	#region Records

	/// <summary>
	/// RecordType
	///		[JsonProperty("type")]
	///		public string Type { get; set; }
	/// </summary>
	public static class RecordType
	{
		public const string Bookmark = "audible.bookmark";
		public const string Clip = "audible.clip";
		public const string LastHeard = "audible.last_heard";
		public const string Note = "audible.note";
	}

	#endregion
}
