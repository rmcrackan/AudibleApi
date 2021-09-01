// INSTRUCTIONS FOR GENERATING
// ===========================
// library api with all response_groups
// https://app.quicktype.io/
// left pane:
//   name=LibraryDtoV10
//   paste in full output of library api with all response_groups
// right pane:
//   namespace: AudibleApi.Common
//   array
//   complete
//   make all properties optional. this adds unneeded stuff (see next step) but also needed stuff
// remove all instances of ", NullValueHandling = NullValueHandling.Ignore"
// PurchaseDate is not optional. remove "?" from type
// Status is not optional. remove "?" from type
// RuntimeLengthMin type: int?
// rename Author class to Person
// class Serialize: add partial
// manually edited lots of enum stuff

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AudibleApi.Common
{
	/// <summary>
	/// Audible API. GET /1.0/library , GET /1.0/library/{asin}
	/// </summary>
	public partial class LibraryDtoV10
	{
		public override string ToString() => $"{Items?.Length ?? 0} {nameof(Items)}, {ResponseGroups?.Length ?? 0} {nameof(ResponseGroups)}";
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

		// tested for
		// 0.3,4.7,8.6 => 0.3f
		// 0.6, 3.5 => 0.6f
		// 5-8 => 5f
		// 5. => 5f
		// abc1.12.def3.4 => 1.12f
		// X.5 => 5f // no leading 0
		private static Regex regex { get; } = new Regex(@"^\D*(?<index>\d+\.?\d*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		public float Index
			=> string.IsNullOrWhiteSpace(Sequence)
			? 0
			: float.Parse(regex.Match(Sequence).Groups["index"].ToString());

		public override string ToString() => $"[{SeriesId}] {SeriesName}";
	}
}
