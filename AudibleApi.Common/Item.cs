using System;
using System.Linq;

namespace AudibleApi.Common
{
	public partial class Item : DtoBase<Item>
	{
		public override string ToString() => $"[{ProductId}] {Title}";

		public string ProductId => Asin;
		public int LengthInMinutes => RuntimeLengthMin ?? 0;
		public string Description => PublisherSummary;
		public bool IsEpisodes
			=> Relationships?.Any(r => (r.RelationshipToProduct == RelationshipToProduct.Child || r.RelationshipToProduct == RelationshipToProduct.Parent) && r.RelationshipType == RelationshipType.Episode)
			?? false;
		/// <summary>
		/// True if this title is an 'Audible Plus' check-out. False if it's owned by the library. More details in comments
		/// </summary>
		#region determining Audible Plus check outs
		// 2020-08-27:
		// tl;dr
		//   usage of codenames (US Minerva) and weird abbrev.s (AYCL, ayce) suggest these are temporary names. expect them to change.
		//   benefit_id == 'AYCL' && @.is_ayce : don't seem to be used any other way, so just check for these
		//
		// long version:
		//
		// item plan with plan_name US Minerva are available in Audible Plus. This is true whether or not it's owned by the account. ie JsonPath:
		//   $.items[?(@.plans[?(@.plan_name == 'US Minerva')])]
		//   'item' {
		//     'plans' [
		//       {
		//         'plan_name': 'US Minerva'
		//
		// if this item is a checkout (ie: not owned by the account), this will also be true:
		//   $.items[?(@.benefit_id == 'AYCL' && @.is_ayce == true)]
		//   'item' {
		//     'benefit_id': 'AYCL',
		//     'is_ayce': true,
		//     'plans' [
		//       {
		//         'plan_name': 'US Minerva'
		//
		// if this item is owned by the account (ie: not a checkout), this will also be true: 
		//   $.items[?(@.benefit_id == null && @.is_ayce == null)]
		//   'item' {
		//     'benefit_id': null,
		//     'is_ayce': null,
		//     'plans' [
		//       {
		//         'plan_name': 'US Minerva'
		#endregion
		public bool IsAudiblePlus => BenefitId != "AYCL" || !IsAyce.HasValue || !IsAyce.Value;
		public string PictureId => ProductImages?.PictureId;
		public DateTime DateAdded => PurchaseDate.UtcDateTime;

		public float Product_OverallStars => Convert.ToSingle(Rating?.OverallDistribution?.DisplayStars ?? 0);
		public float Product_PerformanceStars => Convert.ToSingle(Rating?.PerformanceDistribution?.DisplayStars ?? 0);
		public float Product_StoryStars => Convert.ToSingle(Rating?.StoryDistribution?.DisplayStars ?? 0);

		public int MyUserRating_Overall => Convert.ToInt32(ProvidedReview?.Ratings?.OverallRating ?? 0L);
		public int MyUserRating_Performance => Convert.ToInt32(ProvidedReview?.Ratings?.PerformanceRating ?? 0L);
		public int MyUserRating_Story => Convert.ToInt32(ProvidedReview?.Ratings?.StoryRating ?? 0L);

		public bool IsAbridged => FormatType == AudibleApi.Common.FormatType.Abridged;
		public DateTime? DatePublished => IssueDate?.UtcDateTime; // item.IssueDate == item.ReleaseDate
		public string Publisher => PublisherName;

		// these category properties assume:
		// - we're only exposing 1 category, irrespective of how many the Item actually has
		// - each ladder will have either 1 or 2 levels: parent and optional child
		public Ladder[] Categories => CategoryLadders?.FirstOrDefault()?.Ladder ?? new Ladder[0];
		public Ladder ParentCategory => Categories?.FirstOrDefault();
		public Ladder ChildCategory => Categories.Length > 1 ? Categories[1] : null;

		/// <summary>Add any subtitle after the title title</summary>
		public string TitleWithSubtitle
		{
			get
			{
				var title = Title?.Trim();
				var subtitle = Subtitle?.Trim();
				return !string.IsNullOrWhiteSpace(subtitle)
					? $"{title}: {subtitle}"
					: title;
			}
		}
	}
}
