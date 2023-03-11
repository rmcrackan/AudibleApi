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

using Dinah.Core;
using System;
using System.Linq;

namespace AudibleApi.Common
{
	/// <summary>
	/// Audible API. GET /1.0/library , GET /1.0/library/{asin}
	/// </summary>
	public partial class LibraryDtoV10 : V10Base<LibraryDtoV10>
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

		/// <summary>
		/// If a Book was added to the library as an Audible Plus book, <see cref="Item.IsAyce"/> is true regardless of whether
		/// that title is still available for listening. To listen to it, you need rights under an Audible Plus or
		/// Free Plan. If you don't have either of those plans, then the title will show as "Unavailable" in the
		/// Audible library.
		/// 
		/// <para>Audible Plus Plan Names:</para>
		/// <list type="bullet">
		///		<item>
		///			<term>US Minerva</term>
		///			<description>US-only name</description>
		///		</item>
		///		<item>
		///			<term>Audible-AYCL</term>
		///			<description>Known to work for Italy, and India's Plan name contains "AYCL"</description>
		///		</item>
		/// </list>
		/// <para>Other AYCE Plans:</para>
		/// <list type="bullet">
		///		<item>
		///			<term>Free Tier</term>
		///		</item>
		///		<item>
		///			<term>Ad Enabled Free Tier</term>
		///		</item>
		/// </list>
		/// </summary>
		public bool IsAyce => PlanName.ContainsInsensitive("Minerva") ||
				PlanName.ContainsInsensitive("AYCL") ||
				PlanName.ContainsInsensitive("Free");
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
			=> PictureIDfromUrl(The500?.AbsoluteUri);
		// https://m.media-amazon.com/images/I/51T1NWIkR4L._SL500_.jpg?foo=bar
			
		public string PictureLarge
			=> PictureIDfromUrl(The1215?.AbsoluteUri);

		private static string PictureIDfromUrl(string url)
		{
			if (url is null) return null;
			int lastSlash = url.LastIndexOf("/") + 1;
			int dot = url.IndexOf(".", lastSlash);
			return url.Substring(lastSlash, dot - lastSlash);
		}

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

		/// <summary>Sequence is the original string. Index is the best guess at ordinal position.</summary>
		public float Index => Dinah.Core.StringLib.ExtractFirstNumber(Sequence);

		public override string ToString() => $"[{SeriesId}] {SeriesName}";
	}
}
