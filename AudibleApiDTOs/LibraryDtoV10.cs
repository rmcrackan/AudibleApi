// INSTRUCTIONS FOR GENERATING
// ===========================
// library api with all response_groups
// https://app.quicktype.io/
// left pane:
//   name=LibraryDtoV10
//   paste in full output of library api with all response_groups
// right pane:
//   namespace: AudibleApiDTOs
//   array
//   complete
//   make all properties optional. this adds unneeded stuff (see next step) but also needed stuff
// remove all instances of ", NullValueHandling = NullValueHandling.Ignore"
// PurchaseDate is not optional. remove "?" from type
// Status is not optional. remove "?" from type
// RuntimeLengthMin type: int?
// rename Author class to Person
// class Serialize: add partial
// lots of manually edited enum stuff

using System;
using System.Linq;

namespace AudibleApiDTOs
{
	/// <summary>
	/// Audible API. GET /1.0/library , GET /1.0/library/{asin}
	/// </summary>
	public partial class LibraryDtoV10
	{
		public override string ToString() => $"{Items?.Length ?? 0} {nameof(Items)}, {ResponseGroups?.Length ?? 0} {nameof(ResponseGroups)}";
	}
	public partial class Item
	{
		public string ProductId => Asin;
		public int LengthInMinutes => RuntimeLengthMin ?? 0;
		public string Description => PublisherSummary;
		public bool IsEpisodes
			=> Relationships?.Any(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
			?? false;
		public string PictureId => ProductImages?.PictureId;
		public string SupplementUrl => PdfUrl?.AbsoluteUri;
		public DateTime DateAdded => PurchaseDate.UtcDateTime;

		public float Product_OverallStars => Convert.ToSingle(Rating?.OverallDistribution.DisplayStars ?? 0);
		public float Product_PerformanceStars => Convert.ToSingle(Rating?.PerformanceDistribution.DisplayStars ?? 0);
		public float Product_StoryStars => Convert.ToSingle(Rating?.StoryDistribution.DisplayStars ?? 0);

		public int MyUserRating_Overall => Convert.ToInt32(ProvidedReview?.Ratings.OverallRating ?? 0L);
		public int MyUserRating_Performance => Convert.ToInt32(ProvidedReview?.Ratings.PerformanceRating ?? 0L);
		public int MyUserRating_Story => Convert.ToInt32(ProvidedReview?.Ratings.StoryRating ?? 0L);

		public bool IsAbridged
			=> FormatType.HasValue
			? FormatType == AudibleApiDTOs.FormatType.Abridged
			: false;
		public DateTime? DatePublished => IssueDate?.UtcDateTime; // item.IssueDate == item.ReleaseDate
		public string Publisher => PublisherName;

		// these category properties assume:
		// - we're only exposing 1 category, irrespective of how many the Item actually has
		// - each ladder will have either 1 or 2 levels: parent and optional child
		public Ladder[] Categories => CategoryLadders?.FirstOrDefault()?.Ladder ?? new Ladder[0];
		public Ladder ParentCategory => Categories?.FirstOrDefault();
		public Ladder ChildCategory => Categories.Length > 1 ? Categories[1] : null;

		public override string ToString() => $"[{ProductId}] {Title}";
	}
	public partial class Person
	{
		public override string ToString() => $"{Name}";
	}
	public partial class AvailableCodec
	{
		public override string ToString() => $"{Name} {Format} {EnhancedCodec}";
	}
	public partial class CategoryLadder
	{
		public override string ToString()
		{
			if (Ladder == null)
				return "[null]";
			if (Ladder.Length == 0)
				return "[empty]";
			return Ladder.Select(l => l?.CategoryName).Aggregate((a, b) => $"{a} | {b}");
		}
	}
	public partial class Ladder
	{
		public string CategoryId => Id;
		public string CategoryName => Name;

		public override string ToString() => $"[{CategoryId}] {CategoryName}";
	}
	public partial class ContentRating
	{
		public override string ToString() => $"{Steaminess}";
	}
	public partial class Review
	{
		public override string ToString() => $"{Title}";
	}
	public partial class GuidedResponse
	{
		//public override string ToString() => 
	}
	public partial class Ratings
	{
		public override string ToString() => $"{OverallRating}|{PerformanceRating}|{StoryRating}";
	}
	public partial class ReviewContentScores
	{
		public override string ToString() => $"Helpful={NumHelpfulVotes ?? 0}, Unhelpful={NumUnhelpfulVotes ?? 0}";
	}
	public partial class Plan
	{
		public override string ToString() => $"{PlanName}";
	}
	public partial class Price
	{
		public override string ToString() => $"List={ListPrice}, Lowest={LowestPrice}";
	}
	public partial class ListPriceClass
	{
		public override string ToString() => $"{Base}";
	}
	public partial class ProductImages
	{
		public string PictureId
			=> The500
				?.AbsoluteUri // https://m.media-amazon.com/images/I/51T1NWIkR4L._SL500_.jpg?foo=bar
				.Split('/').Last() // 51T1NWIkR4L._SL500_.jpg?foo=bar
				.Split('.').First() // 51T1NWIkR4L
			;

		public override string ToString() => $"{The500?.AbsoluteUri}";
	}
	public partial class Rating
	{
		public override string ToString() => $"{OverallDistribution}|{PerformanceDistribution}|{StoryDistribution}";
	}
	public partial class Distribution
	{
		public override string ToString() => $"{DisplayStars:0.0}";
	}
	public partial class Relationship
	{
		public override string ToString() => $"{RelationshipToProduct} {RelationshipType}";
	}
	public partial class Series
	{
		public string SeriesName => Title;
		public string SeriesId => Asin;
		public float Index
			=> string.IsNullOrWhiteSpace(Sequence)
			? 0
			// eg: a book containing volumes 5,6,7,8 has sequence "5-8"
			: float.Parse(Sequence.Split('-').First());

		public override string ToString() => $"[{SeriesId}] {SeriesName}";
	}
}
