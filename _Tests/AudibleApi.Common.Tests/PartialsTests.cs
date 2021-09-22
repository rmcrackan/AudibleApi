using System;
using System.Linq;
using System.Collections.Generic;
using AudibleApi.Common;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PartialsTests
{
	[TestClass]
	public class LibraryDtoV10_ToString
	{
		//public override string ToString() => $"{Items.Length} {nameof(Items)}, {ResponseGroups.Length} {nameof(ResponseGroups)}";
		[TestMethod]
		public void only_null()
			=> new LibraryDtoV10 { Items = null, ResponseGroups = null }.ToString().Should().Be("0 Items, 0 ResponseGroups");

		[TestMethod]
		public void only_empty()
			=> new LibraryDtoV10 { Items = new Item[0], ResponseGroups = new string[0] }.ToString().Should().Be("0 Items, 0 ResponseGroups");

		[TestMethod]
		public void has_items()
			=> new LibraryDtoV10 { Items = new[] { new Item(), new Item() }, ResponseGroups = new[] { "a", "b" } }.ToString().Should().Be("2 Items, 2 ResponseGroups");
	}

	[TestClass]
	public class Item_Fields
	{
		[TestMethod]
		public void ProductId_null() => new Item { Asin = null }.ProductId.Should().BeNull();
		[TestMethod]
		public void ProductId_empty() => new Item { Asin = "" }.ProductId.Should().Be("");
		[TestMethod]
		public void ProductId_whitespace() => new Item { Asin = "   " }.ProductId.Should().Be("   ");
		[TestMethod]
		public void ProductId_value() => new Item { Asin = "foo" }.ProductId.Should().Be("foo");

		[TestMethod]
		public void LengthInMinutes_null() => new Item { RuntimeLengthMin = null }.LengthInMinutes.Should().Be(0);
		[TestMethod]
		public void LengthInMinutes_value() => new Item { RuntimeLengthMin = 123 }.LengthInMinutes.Should().Be(123);

		[TestMethod]
		public void Description_null() => new Item { PublisherSummary = null }.Description.Should().BeNull();
		[TestMethod]
		public void Description_empty() => new Item { PublisherSummary = "" }.Description.Should().Be("");
		[TestMethod]
		public void Description_whitespace() => new Item { PublisherSummary = "   " }.Description.Should().Be("   ");
		[TestMethod]
		public void Description_value() => new Item { PublisherSummary = "foo" }.Description.Should().Be("foo");

		[TestMethod]
		public void IsEpisodes_Relationships_null() => new Item { Relationships = null }.IsEpisodes.Should().BeFalse();
		[TestMethod]
		public void IsEpisodes_Relationships_empty() => new Item { Relationships = new Relationship[0] }.IsEpisodes.Should().BeFalse();
		[TestMethod]
		public void IsEpisodes_no_child() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship { RelationshipType = RelationshipType.Episode }
			}
		}.IsEpisodes.Should().BeFalse();
		[TestMethod]
		public void IsEpisodes_no_episode() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship { RelationshipToProduct = RelationshipToProduct.Child }
			}
		}.IsEpisodes.Should().BeFalse();
		[TestMethod]
		public void IsEpisodes_true() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship
				{
					RelationshipType = RelationshipType.Episode,
					RelationshipToProduct = RelationshipToProduct.Child
				}
			}
		}.IsEpisodes.Should().BeTrue();

		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);
		[TestMethod]
		public void PictureId_null_ProductImages() => new Item { ProductImages = null }.PictureId.Should().BeNull();
		[TestMethod]
		public void PictureId_null_The500() => new Item { ProductImages = new ProductImages { The500 = null } }.PictureId.Should().BeNull();
		[TestMethod]
		public void PictureId_populated() => new Item { ProductImages = new ProductImages { The500 = uri } }.PictureId.Should().Be("t");

		DateTime dt { get; } = new DateTime(2000, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc);
		[TestMethod]
		public void DateAdded() => new Item { PurchaseDate = new DateTimeOffset(dt) }.DateAdded.Should().Be(dt);

		Rating rating { get; } = new Rating
		{
			OverallDistribution = new Distribution { DisplayStars = 1.23 },
			PerformanceDistribution = new Distribution { DisplayStars = 4.56 },
			StoryDistribution = new Distribution { DisplayStars = 7.89 }
		};
		[TestMethod]
		public void Product_OverallStars_null_Rating() => new Item { Rating = null }.Product_OverallStars.Should().Be(0);
		[TestMethod]
		public void Product_OverallStars_populated() => new Item { Rating = rating }.Product_OverallStars.Should().Be(1.23f);
		[TestMethod]
		public void Product_PerformanceStars_null_Rating() => new Item { Rating = null }.Product_PerformanceStars.Should().Be(0);
		[TestMethod]
		public void Product_PerformanceStars_populated() => new Item { Rating = rating }.Product_PerformanceStars.Should().Be(4.56f);
		[TestMethod]
		public void Product_StoryStars_null_Rating() => new Item { Rating = null }.Product_StoryStars.Should().Be(0);
		[TestMethod]
		public void Product_StoryStars_populated() => new Item { Rating = rating }.Product_StoryStars.Should().Be(7.89f);

		Review review { get; } = new Review
		{
			Ratings = new Ratings
			{
				OverallRating = 1,
				PerformanceRating = 2,
				StoryRating = 3
			}
		};
		[TestMethod]
		public void MyUserRating_Overall_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Overall.Should().Be(0);
		[TestMethod]
		public void MyUserRating_Overall_populated() => new Item { ProvidedReview = review }.MyUserRating_Overall.Should().Be(1);
		[TestMethod]
		public void MyUserRating_Performance_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Performance.Should().Be(0);
		[TestMethod]
		public void MyUserRating_Performance_populated() => new Item { ProvidedReview = review }.MyUserRating_Performance.Should().Be(2);
		[TestMethod]
		public void MyUserRating_Story_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Story.Should().Be(0);
		[TestMethod]
		public void MyUserRating_Story_populated() => new Item { ProvidedReview = review }.MyUserRating_Story.Should().Be(3);

		[TestMethod]
		public void IsAbridged_null_FormatType() => new Item { FormatType = null }.IsAbridged.Should().BeFalse();
		[TestMethod]
		public void IsAbridged_FormatType_not_Abridged() => new Item { FormatType = FormatType.Unabridged }.IsAbridged.Should().BeFalse();
		[TestMethod]
		public void IsAbridged_FormatType_Abridged() => new Item { FormatType = FormatType.Abridged }.IsAbridged.Should().BeTrue();

		[TestMethod]
		public void DatePublished_null_IssueDate() => new Item { IssueDate = null }.DatePublished.Should().BeNull();
		[TestMethod]
		public void DatePublished_populated() => new Item { IssueDate = new DateTimeOffset(dt) }.DatePublished.Should().Be(dt);

		[TestMethod]
		public void Publisher_null() => new Item { PublisherName = null }.Publisher.Should().BeNull();
		[TestMethod]
		public void Publisher_empty() => new Item { PublisherName = "" }.Publisher.Should().Be("");
		[TestMethod]
		public void Publisher_whitespace() => new Item { PublisherName = "   " }.Publisher.Should().Be("   ");
		[TestMethod]
		public void Publisher_value() => new Item { PublisherName = "foo" }.Publisher.Should().Be("foo");

		[TestMethod]
		public void Categories_null_CategoryLadders() => new Item { CategoryLadders = null }.Categories.Length.Should().Be(0);
		[TestMethod]
		public void Categories_empty_CategoryLadders() => new Item { CategoryLadders = new CategoryLadder[0] }.Categories.Length.Should().Be(0);
		[TestMethod]
		public void Categories_empty_Ladder_set() => new Item
		{
			CategoryLadders = new CategoryLadder[]
			{
				new CategoryLadder { Ladder = new Ladder[0] }
			}
		}
		.Categories.Count().Should().Be(0);
		[TestMethod]
		public void Categories_1_Ladder()
		{
			var item = new Item
			{
				CategoryLadders = new CategoryLadder[]
				{
					new CategoryLadder
					{
						Ladder = new[] { new Ladder { Name = "a" } }
					}
				}
			};
			var ladders = item.Categories;
			ladders.Count().Should().Be(1);
			ladders[0].Name.Should().Be("a");
		}
		[TestMethod]
		public void Categories_2_Ladders()
		{
			var item = new Item
			{
				CategoryLadders = new CategoryLadder[]
				{
					new CategoryLadder
					{
						Ladder = new[]
						{
							new Ladder { Name = "a" },
							new Ladder { Name = "b" }
						}
					}
				}
			};
			var ladders = item.Categories;
			ladders.Count().Should().Be(2);
			ladders[0].Name.Should().Be("a");
			ladders[1].Name.Should().Be("b");
		}
		[TestMethod]
		public void Categories_ignore_2nd_CategoryLadder()
		{
			var item = new Item
			{
				CategoryLadders = new CategoryLadder[]
				{
					new CategoryLadder
					{
						Ladder = new[] { new Ladder { Name = "a" }, new Ladder { Name = "b" } }
					},
					new CategoryLadder
					{
						Ladder = new[] { new Ladder { Name = "zzzz" }, new Ladder { Name = "yyyy" } }
					}
				}
			};
			var ladders = item.Categories;
			ladders.Count().Should().Be(2);
			ladders[0].Name.Should().Be("a");
			ladders[1].Name.Should().Be("b");
		}

		[TestMethod]
		public void ParentCategory_null_Categories() => new Item { CategoryLadders = null }.ParentCategory.Should().BeNull();
		[TestMethod]
		public void ParentCategory_1_Category() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" } } } } }.ParentCategory.Name.Should().Be("a");
		[TestMethod]
		public void ParentCategory_get_1st_category() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" }, new Ladder { Name = "xxxx" } } } } }.ParentCategory.Name.Should().Be("a");

		[TestMethod]
		public void ChildCategory_0_categories_returns_null() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new Ladder[0] } } }.ChildCategory.Should().BeNull();
		[TestMethod]
		public void ChildCategory_1_category_returns_null() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" } } } } }.ChildCategory.Should().BeNull();
		[TestMethod]
		public void ChildCategory_2_categories_returns_2nd() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" }, new Ladder { Name = "a" } } } } }.ChildCategory.Name.Should().Be("a");
		[TestMethod]
		public void ChildCategory_3_categories_returns_2nd() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" }, new Ladder { Name = "a" }, new Ladder { Name = "zzzz" } } } } }.ChildCategory.Name.Should().Be("a");
	}

	[TestClass]
	public class Item_ToString
	{
		[TestMethod]
		public void Asin_null() => new Item { Asin = null, Title = "a" }.ToString().Should().Be("[] a");
		[TestMethod]
		public void Title_null() => new Item { Asin = "a", Title = null }.ToString().Should().Be("[a] ");
		[TestMethod]
		public void populated() => new Item { Asin = "a", Title = "b" }.ToString().Should().Be("[a] b");
	}

	[TestClass]
	public class Person_ToString
	{
		[TestMethod]
		public void Name_null() => new Person { Name = null }.ToString().Should().Be("");
		[TestMethod]
		public void Name_populated() => new Person { Name = "a" }.ToString().Should().Be("a");
	}

	[TestClass]
	public class AvailableCodec_ToString
	{
		[TestMethod]
		public void Name_null() => new AvailableCodec { Name = null, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = EnhancedCodec.Aax }.ToString().Should().Be(" Enhanced aax");
		[TestMethod]
		public void Format_null() => new AvailableCodec { Name = Name.Aax, Format = null, EnhancedCodec = EnhancedCodec.Aax }.ToString().Should().Be("aax  aax");
		[TestMethod]
		public void EnhancedCodec_null() => new AvailableCodec { Name = Name.Aax, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = null }.ToString().Should().Be("aax Enhanced ");
		[TestMethod]
		public void populated() => new AvailableCodec { Name = Name.Aax, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = EnhancedCodec.Aax }.ToString().Should().Be("aax Enhanced aax");
	}

	[TestClass]
	public class CategoryLadder_ToString
	{
		[TestMethod]
		public void Ladder_set_null() => new CategoryLadder { Ladder = null }.ToString().Should().Be("[null]");
		[TestMethod]
		public void Ladder_set_empty() => new CategoryLadder { Ladder = new Ladder[0] }.ToString().Should().Be("[empty]");
		[TestMethod]
		public void Ladder_1_null() => new CategoryLadder { Ladder = new Ladder[] { null } }.ToString().Should().BeNull();
		[TestMethod]
		public void Ladder_2_nulls() => new CategoryLadder { Ladder = new Ladder[] { null, null } }.ToString().Should().Be(" | ");
		[TestMethod]
		public void Ladder_1_populated_null_Name() => new CategoryLadder { Ladder = new[] { new Ladder { Name = null } } }.ToString().Should().BeNull();
		[TestMethod]
		public void Ladder_1_populated() => new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" } } }.ToString().Should().Be("a");
		[TestMethod]
		public void Ladder_1_populated_1_null() => new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" }, null } }.ToString().Should().Be("a | ");
		[TestMethod]
		public void CategoryName_2_values() => new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" }, new Ladder { Name = "b" } } }.ToString().Should().Be("a | b");
	}

	[TestClass]
	public class Ladder_Fields
	{
		[TestMethod]
		public void CategoryId_null() => new Ladder { Id = null }.CategoryId.Should().BeNull();
		[TestMethod]
		public void CategoryId_empty() => new Ladder { Id = "" }.CategoryId.Should().Be("");
		[TestMethod]
		public void CategoryId_whitespace() => new Ladder { Id = "   " }.CategoryId.Should().Be("   ");
		[TestMethod]
		public void CategoryId_value() => new Ladder { Id = "foo" }.CategoryId.Should().Be("foo");

		[TestMethod]
		public void CategoryName_null() => new Ladder { Name = null }.CategoryName.Should().BeNull();
		[TestMethod]
		public void CategoryName_empty() => new Ladder { Name = "" }.CategoryName.Should().Be("");
		[TestMethod]
		public void CategoryName_whitespace() => new Ladder { Name = "   " }.CategoryName.Should().Be("   ");
		[TestMethod]
		public void CategoryName_value() => new Ladder { Name = "foo" }.CategoryName.Should().Be("foo");
	}

	[TestClass]
	public class Ladder_ToString
	{
		[TestMethod]
		public void Id_null() => new Ladder { Id = null, Name = "n" }.ToString().Should().Be("[] n");
		[TestMethod]
		public void Name_null() => new Ladder { Id = "i", Name = null }.ToString().Should().Be("[i] ");
		[TestMethod]
		public void populated() => new Ladder { Id = "i", Name = "n" }.ToString().Should().Be("[i] n");
	}

	[TestClass]
	public class ContentRating_ToString
	{
		[TestMethod]
		public void Steaminess_null() => new ContentRating { Steaminess = null }.ToString().Should().Be("");
		[TestMethod]
		public void Steaminess_value() => new ContentRating { Steaminess = 123 }.ToString().Should().Be("123");
	}

	[TestClass]
	public class Review_ToString
	{
		[TestMethod]
		public void Title_null() => new Review { Title = null }.ToString().Should().Be("");
		[TestMethod]
		public void Title_empty() => new Review { Title = "" }.ToString().Should().Be("");
		[TestMethod]
		public void Title_whitespace() => new Review { Title = "   " }.ToString().Should().Be("   ");
		[TestMethod]
		public void Title_value() => new Review { Title = "foo" }.ToString().Should().Be("foo");
	}

	[TestClass]
	public class Ratings_ToString
	{
		[TestMethod]
		public void OverallRating_null() => new Ratings { OverallRating = null, PerformanceRating = 2, StoryRating = 3 }.ToString().Should().Be("|2|3");
		[TestMethod]
		public void PerformanceRating_null() => new Ratings { OverallRating = 1, PerformanceRating = null, StoryRating = 3 }.ToString().Should().Be("1||3");
		[TestMethod]
		public void StoryRating_null() => new Ratings { OverallRating = 1, PerformanceRating = 2, StoryRating = null }.ToString().Should().Be("1|2|");
		[TestMethod]
		public void populated() => new Ratings { OverallRating = 1, PerformanceRating = 2, StoryRating = 3 }.ToString().Should().Be("1|2|3");
	}

	[TestClass]
	public class ReviewContentScores_ToString
	{
		[TestMethod]
		public void NumHelpfulVotes_null() => new ReviewContentScores { NumHelpfulVotes = null, NumUnhelpfulVotes = 9 }.ToString().Should().Be("Helpful=0, Unhelpful=9");
		[TestMethod]
		public void NumUnhelpfulVotes_null() => new ReviewContentScores { NumHelpfulVotes = 1, NumUnhelpfulVotes = null }.ToString().Should().Be("Helpful=1, Unhelpful=0");
		[TestMethod]
		public void populated() => new ReviewContentScores { NumHelpfulVotes = 1, NumUnhelpfulVotes = 9 }.ToString().Should().Be("Helpful=1, Unhelpful=9");
	}

	[TestClass]
	public class Plan_ToString
	{
		[TestMethod]
		public void PlanName_null() => new Plan { PlanName = null }.ToString().Should().Be("");
		[TestMethod]
		public void populated() => new Plan { PlanName = PlanName.Radio }.ToString().Should().Be("Radio");
	}

	[TestClass]
	public class Price_ToString
	{
		[TestMethod]
		public void ListPrice_null() => new Price { ListPrice = null, LowestPrice = new ListPriceClass { Base = 1.2 } }.ToString().Should().Be("List=, Lowest=1.2");
		[TestMethod]
		public void LowestPrice_null() => new Price { ListPrice = new ListPriceClass { Base = 8.9 }, LowestPrice = null }.ToString().Should().Be("List=8.9, Lowest=");
		[TestMethod]
		public void populated() => new Price { ListPrice = new ListPriceClass { Base = 8.9 }, LowestPrice = new ListPriceClass { Base = 1.2 } }.ToString().Should().Be("List=8.9, Lowest=1.2");
	}

	[TestClass]
	public class ListPriceClass_ToString
	{
		[TestMethod]
		public void Base_null() => new ListPriceClass { Base = null }.ToString().Should().Be("");
		[TestMethod]
		public void populated() => new ListPriceClass { Base = 1.2 }.ToString().Should().Be("1.2");
	}

	[TestClass]
	public class ProductImages_Fields
	{
		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);

		[TestMethod]
		public void PictureId_The500_null() => new ProductImages { The500 = null }.PictureId.Should().BeNull();
		[TestMethod]
		public void PictureId_populated() => new ProductImages { The500 = uri }.PictureId.Should().Be("t");
	}

	[TestClass]
	public class ProductImages_ToString
	{
		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);

		[TestMethod]
		public void The500_null() => new ProductImages { The500 = null }.ToString().Should().Be("");
		[TestMethod]
		public void populated() => new ProductImages { The500 = uri }.ToString().Should().Be(uriPath);
	}

	[TestClass]
	public class Rating_ToString
	{
		[TestMethod]
		public void OverallDistribution_null() => new Rating { OverallDistribution = null, PerformanceDistribution = new Distribution { DisplayStars = 4.56 }, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().Should().Be("|4.6|7.9");
		[TestMethod]
		public void PerformanceDistribution_null() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = null, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().Should().Be("1.2||7.9");
		[TestMethod]
		public void StoryDistribution_null() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = new Distribution { DisplayStars = 4.56 }, StoryDistribution = null }.ToString().Should().Be("1.2|4.6|");
		[TestMethod]
		public void populated() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = new Distribution { DisplayStars = 4.56}, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().Should().Be("1.2|4.6|7.9");
	}

	[TestClass]
	public class Distribution_ToString
	{
		[TestMethod]
		public void DisplayStars_null() => new Distribution { DisplayStars = null }.ToString().Should().Be("");
		[TestMethod]
		public void populated() => new Distribution { DisplayStars = 4.56 }.ToString().Should().Be("4.6");
	}

	[TestClass]
	public class Relationship_ToString
	{
		[TestMethod]
		public void RelationshipToProduct_null() => new Relationship { RelationshipToProduct = null, RelationshipType = RelationshipType.Season }.ToString().Should().Be(" season");
		[TestMethod]
		public void RelationshipType_null() => new Relationship { RelationshipToProduct = RelationshipToProduct.Child, RelationshipType = null }.ToString().Should().Be("child ");
		[TestMethod]
		public void populated() => new Relationship { RelationshipToProduct = RelationshipToProduct.Child, RelationshipType = RelationshipType.Season }.ToString().Should().Be("child season");
	}

	[TestClass]
	public class Series_ToString
	{
		[TestMethod]
		public void null_Asin() => new Series { Asin = null, Title = "t" }.ToString().Should().Be("[] t");
		[TestMethod]
		public void null_Title() => new Series { Asin = "a", Title = null }.ToString().Should().Be("[a] ");
		[TestMethod]
		public void populated() => new Series { Asin = "a", Title = "t" }.ToString().Should().Be("[a] t");
	}
}
