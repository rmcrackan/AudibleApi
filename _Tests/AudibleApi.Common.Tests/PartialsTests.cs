namespace PartialsTests
{
	[TestClass]
	public class LibraryDtoV10_ToString
	{
		//public override string ToString() => $"{Items.Length} {nameof(Items)}, {ResponseGroups.Length} {nameof(ResponseGroups)}";
		[TestMethod]
		public void only_null()
			=> new LibraryDtoV10 { Items = null, ResponseGroups = null }.ToString().ShouldBe("0 Items, 0 ResponseGroups");

		[TestMethod]
		public void only_empty()
			=> new LibraryDtoV10 { Items = new Item[0], ResponseGroups = new string[0] }.ToString().ShouldBe("0 Items, 0 ResponseGroups");

		[TestMethod]
		public void has_items()
			=> new LibraryDtoV10 { Items = new[] { new Item(), new Item() }, ResponseGroups = new[] { "a", "b" } }.ToString().ShouldBe("2 Items, 2 ResponseGroups");
	}

	[TestClass]
	public class Item_Fields
	{
		[TestMethod]
		public void ProductId_null() => new Item { Asin = null }.ProductId.ShouldBeNull();
		[TestMethod]
		public void ProductId_empty() => new Item { Asin = "" }.ProductId.ShouldBe("");
		[TestMethod]
		public void ProductId_whitespace() => new Item { Asin = "   " }.ProductId.ShouldBe("   ");
		[TestMethod]
		public void ProductId_value() => new Item { Asin = "foo" }.ProductId.ShouldBe("foo");

		[TestMethod]
		public void LengthInMinutes_null() => new Item { RuntimeLengthMin = null }.LengthInMinutes.ShouldBe(0);
		[TestMethod]
		public void LengthInMinutes_value() => new Item { RuntimeLengthMin = 123 }.LengthInMinutes.ShouldBe(123);

		[TestMethod]
		public void Description_null() => new Item { PublisherSummary = null }.Description.ShouldBeNull();
		[TestMethod]
		public void Description_empty() => new Item { PublisherSummary = "" }.Description.ShouldBe("");
		[TestMethod]
		public void Description_whitespace() => new Item { PublisherSummary = "   " }.Description.ShouldBe("   ");
		[TestMethod]
		public void Description_value() => new Item { PublisherSummary = "foo" }.Description.ShouldBe("foo");

		[TestMethod]
		public void IsEpisodes_Relationships_null() => new Item { Relationships = null }.IsEpisodes.ShouldBeFalse();
		[TestMethod]
		public void IsEpisodes_Relationships_empty() => new Item { Relationships = new Relationship[0] }.IsEpisodes.ShouldBeFalse();
		[TestMethod]
		public void IsEpisodes_no_child() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship { RelationshipType = RelationshipType.Episode }
			}
		}.IsEpisodes.ShouldBeFalse();
		[TestMethod]
		public void IsEpisodes_no_episode() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship { RelationshipToProduct = RelationshipToProduct.Child }
			}
		}.IsSeriesParent.ShouldBeFalse();
		[TestMethod]
		public void IsEpisodes_true() => new Item
		{
			Relationships = new Relationship[]
			{
				new Relationship
				{
					RelationshipType = RelationshipType.Episode,
					RelationshipToProduct = RelationshipToProduct.Parent
				}
			}
		}.IsEpisodes.ShouldBeTrue();

		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);
		[TestMethod]
		public void PictureId_null_ProductImages() => new Item { ProductImages = null }.PictureId.ShouldBeNull();
		[TestMethod]
		public void PictureId_null_The500() => new Item { ProductImages = new ProductImages { The500 = null } }.PictureId.ShouldBeNull();
		[TestMethod]
		public void PictureId_populated() => new Item { ProductImages = new ProductImages { The500 = uri } }.PictureId.ShouldBe("t");

		DateTime dt { get; } = new DateTime(2000, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc);
		[TestMethod]
		public void DateAdded() => new Item { PurchaseDate = new DateTimeOffset(dt) }.DateAdded.ShouldBe(dt);

		Rating rating { get; } = new Rating
		{
			OverallDistribution = new Distribution { DisplayStars = 1.23 },
			PerformanceDistribution = new Distribution { DisplayStars = 4.56 },
			StoryDistribution = new Distribution { DisplayStars = 7.89 }
		};
		[TestMethod]
		public void Product_OverallStars_null_Rating() => new Item { Rating = null }.Product_OverallStars.ShouldBe(0);
		[TestMethod]
		public void Product_OverallStars_populated() => new Item { Rating = rating }.Product_OverallStars.ShouldBe(1.23f);
		[TestMethod]
		public void Product_PerformanceStars_null_Rating() => new Item { Rating = null }.Product_PerformanceStars.ShouldBe(0);
		[TestMethod]
		public void Product_PerformanceStars_populated() => new Item { Rating = rating }.Product_PerformanceStars.ShouldBe(4.56f);
		[TestMethod]
		public void Product_StoryStars_null_Rating() => new Item { Rating = null }.Product_StoryStars.ShouldBe(0);
		[TestMethod]
		public void Product_StoryStars_populated() => new Item { Rating = rating }.Product_StoryStars.ShouldBe(7.89f);

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
		public void MyUserRating_Overall_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Overall.ShouldBe(0);
		[TestMethod]
		public void MyUserRating_Overall_populated() => new Item { ProvidedReview = review }.MyUserRating_Overall.ShouldBe(1);
		[TestMethod]
		public void MyUserRating_Performance_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Performance.ShouldBe(0);
		[TestMethod]
		public void MyUserRating_Performance_populated() => new Item { ProvidedReview = review }.MyUserRating_Performance.ShouldBe(2);
		[TestMethod]
		public void MyUserRating_Story_null_Rating() => new Item { ProvidedReview = null }.MyUserRating_Story.ShouldBe(0);
		[TestMethod]
		public void MyUserRating_Story_populated() => new Item { ProvidedReview = review }.MyUserRating_Story.ShouldBe(3);

		[TestMethod]
		public void IsAbridged_null_FormatType() => new Item { FormatType = null }.IsAbridged.ShouldBeFalse();
		[TestMethod]
		public void IsAbridged_FormatType_not_Abridged() => new Item { FormatType = FormatType.Unabridged }.IsAbridged.ShouldBeFalse();
		[TestMethod]
		public void IsAbridged_FormatType_Abridged() => new Item { FormatType = FormatType.Abridged }.IsAbridged.ShouldBeTrue();

		[TestMethod]
		public void DatePublished_null_IssueDate() => new Item { IssueDate = null }.DatePublished.ShouldBeNull();
		[TestMethod]
		public void DatePublished_populated() => new Item { IssueDate = new DateTimeOffset(dt) }.DatePublished.ShouldBe(dt);

		[TestMethod]
		public void Publisher_null() => new Item { PublisherName = null }.Publisher.ShouldBeNull();
		[TestMethod]
		public void Publisher_empty() => new Item { PublisherName = "" }.Publisher.ShouldBe("");
		[TestMethod]
		public void Publisher_whitespace() => new Item { PublisherName = "   " }.Publisher.ShouldBe("   ");
		[TestMethod]
		public void Publisher_value() => new Item { PublisherName = "foo" }.Publisher.ShouldBe("foo");

		[TestMethod]
		public void Categories_null_CategoryLadders()
		{
			var categories = new Item { CategoryLadders = null }.Categories;
			categories.ShouldNotBeNull();
			categories.Length.ShouldBe(0);
		}
		[TestMethod]
		public void Categories_empty_CategoryLadders()
		{
			var categories = new Item { CategoryLadders = new CategoryLadder[0] }.Categories;
			categories.ShouldNotBeNull();
			categories.Length.ShouldBe(0);
		}
		[TestMethod]
		public void Categories_empty_Ladder_set()
		{
			var categories = new Item { CategoryLadders = [new CategoryLadder { Ladder = [] }] }.Categories;
			categories.ShouldNotBeNull();
			categories.Length.ShouldBe(0);
		}
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
			ladders.ShouldNotBeNull();
			ladders.Count().ShouldBe(1);
			ladders[0].ShouldNotBeNull();
			ladders[0]!.Name.ShouldBe("a");
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
			ladders.ShouldNotBeNull();
			ladders.Count().ShouldBe(2);
			ladders[0].ShouldNotBeNull();
			ladders[1].ShouldNotBeNull();
			ladders[0]!.Name.ShouldBe("a");
			ladders[1]!.Name.ShouldBe("b");
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
			ladders.ShouldNotBeNull();
			ladders.Count().ShouldBe(2);
			ladders[0].ShouldNotBeNull();
			ladders[1].ShouldNotBeNull();
			ladders[0]!.Name.ShouldBe("a");
			ladders[1]!.Name.ShouldBe("b");
		}

		[TestMethod]
		public void ParentCategory_null_Categories() => new Item { CategoryLadders = null }.ParentCategory.ShouldBeNull();
		[TestMethod]
		public void ParentCategory_1_Category()
		{
			var parentCategory = new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" } } } } }.ParentCategory;
			parentCategory.ShouldNotBeNull();
			parentCategory.Name.ShouldBe("a");
		}
		[TestMethod]
		public void ParentCategory_get_1st_category()
		{
			var parentCategory = new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "a" }, new Ladder { Name = "xxxx" } } } } }.ParentCategory;
			parentCategory.ShouldNotBeNull();
			parentCategory.Name.ShouldBe("a");
		}

		[TestMethod]
		public void ChildCategory_0_categories_returns_null() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new Ladder[0] } } }.ChildCategory.ShouldBeNull();
		[TestMethod]
		public void ChildCategory_1_category_returns_null() => new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" } } } } }.ChildCategory.ShouldBeNull();
		[TestMethod]
		public void ChildCategory_2_categories_returns_2nd()
		{
			var childCategory = new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" }, new Ladder { Name = "a" } } } } }.ChildCategory;
			childCategory.ShouldNotBeNull();
			childCategory.Name.ShouldBe("a");
		}
		[TestMethod]
		public void ChildCategory_3_categories_returns_2nd()
		{
			var childCategory = new Item { CategoryLadders = new[] { new CategoryLadder { Ladder = new[] { new Ladder { Name = "xxxx" }, new Ladder { Name = "a" }, new Ladder { Name = "zzzz" } } } } }.ChildCategory;
			childCategory.ShouldNotBeNull();
			childCategory.Name.ShouldBe("a");
		}
	}

	[TestClass]
	public class Item_ToString
	{
		[TestMethod]
		public void Asin_null() => new Item { Asin = null, Title = "a" }.ToString().ShouldBe("[] a");
		[TestMethod]
		public void Title_null() => new Item { Asin = "a", Title = null }.ToString().ShouldBe("[a] ");
		[TestMethod]
		public void populated() => new Item { Asin = "a", Title = "b" }.ToString().ShouldBe("[a] b");
	}

	[TestClass]
	public class Person_ToString
	{
		[TestMethod]
		public void Name_null() => new Person { Name = null }.ToString().ShouldBe("");
		[TestMethod]
		public void Name_populated() => new Person { Name = "a" }.ToString().ShouldBe("a");
	}

	[TestClass]
	public class AvailableCodec_ToString
	{
		[TestMethod]
		public void Name_null() => new AvailableCodec { Name = null, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = EnhancedCodec.Aax }.ToString().ShouldBe(" Enhanced aax");
		[TestMethod]
		public void Format_null() => new AvailableCodec { Name = Name.Aax, Format = null, EnhancedCodec = EnhancedCodec.Aax }.ToString().ShouldBe("aax  aax");
		[TestMethod]
		public void EnhancedCodec_null() => new AvailableCodec { Name = Name.Aax, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = null }.ToString().ShouldBe("aax Enhanced ");
		[TestMethod]
		public void populated() => new AvailableCodec { Name = Name.Aax, Format = AvailableCodecFormat.Enhanced, EnhancedCodec = EnhancedCodec.Aax }.ToString().ShouldBe("aax Enhanced aax");
	}

	[TestClass]
	public class CategoryLadder_ToString
	{
		[TestMethod]
		public void Ladder_set_null() => new CategoryLadder { Ladder = null }.ToString().ShouldBe("[null]");
		[TestMethod]
		public void Ladder_set_empty() => new CategoryLadder { Ladder = [] }.ToString().ShouldBe("[empty]");
		[TestMethod]
		public void Ladder_1_null() => new CategoryLadder { Ladder = [null] }.ToString().ShouldBeNull();
		[TestMethod]
		public void Ladder_2_nulls() => new CategoryLadder { Ladder = [null, null] }.ToString().ShouldBe(" | ");
		[TestMethod]
		public void Ladder_1_populated_null_Name() => new CategoryLadder { Ladder = [new Ladder { Name = null }] }.ToString().ShouldBeNull();
		[TestMethod]
		public void Ladder_1_populated() => new CategoryLadder { Ladder = [new Ladder { Name = "a" }] }.ToString().ShouldBe("a");
		[TestMethod]
		public void Ladder_1_populated_1_null() => new CategoryLadder { Ladder = [new Ladder { Name = "a" }, null] }.ToString().ShouldBe("a | ");
		[TestMethod]
		public void CategoryName_2_values() => new CategoryLadder { Ladder = [new Ladder { Name = "a" }, new Ladder { Name = "b" }] }.ToString().ShouldBe("a | b");
	}

	[TestClass]
	public class Ladder_Fields
	{
		[TestMethod]
		public void CategoryId_null() => new Ladder { Id = null }.CategoryId.ShouldBeNull();
		[TestMethod]
		public void CategoryId_empty() => new Ladder { Id = "" }.CategoryId.ShouldBe("");
		[TestMethod]
		public void CategoryId_whitespace() => new Ladder { Id = "   " }.CategoryId.ShouldBe("   ");
		[TestMethod]
		public void CategoryId_value() => new Ladder { Id = "foo" }.CategoryId.ShouldBe("foo");

		[TestMethod]
		public void CategoryName_null() => new Ladder { Name = null }.CategoryName.ShouldBeNull();
		[TestMethod]
		public void CategoryName_empty() => new Ladder { Name = "" }.CategoryName.ShouldBe("");
		[TestMethod]
		public void CategoryName_whitespace() => new Ladder { Name = "   " }.CategoryName.ShouldBe("   ");
		[TestMethod]
		public void CategoryName_value() => new Ladder { Name = "foo" }.CategoryName.ShouldBe("foo");
	}

	[TestClass]
	public class Ladder_ToString
	{
		[TestMethod]
		public void Id_null() => new Ladder { Id = null, Name = "n" }.ToString().ShouldBe("[] n");
		[TestMethod]
		public void Name_null() => new Ladder { Id = "i", Name = null }.ToString().ShouldBe("[i] ");
		[TestMethod]
		public void populated() => new Ladder { Id = "i", Name = "n" }.ToString().ShouldBe("[i] n");
	}

	[TestClass]
	public class ContentRating_ToString
	{
		[TestMethod]
		public void Steaminess_null() => new ContentRating { Steaminess = null }.ToString().ShouldBe("");
		[TestMethod]
		public void Steaminess_value() => new ContentRating { Steaminess = 123 }.ToString().ShouldBe("123");
	}

	[TestClass]
	public class Review_ToString
	{
		[TestMethod]
		public void Title_null() => new Review { Title = null }.ToString().ShouldBe("");
		[TestMethod]
		public void Title_empty() => new Review { Title = "" }.ToString().ShouldBe("");
		[TestMethod]
		public void Title_whitespace() => new Review { Title = "   " }.ToString().ShouldBe("   ");
		[TestMethod]
		public void Title_value() => new Review { Title = "foo" }.ToString().ShouldBe("foo");
	}

	[TestClass]
	public class Ratings_ToString
	{
		[TestMethod]
		public void OverallRating_null() => new Ratings { OverallRating = null, PerformanceRating = 2, StoryRating = 3 }.ToString().ShouldBe("|2|3");
		[TestMethod]
		public void PerformanceRating_null() => new Ratings { OverallRating = 1, PerformanceRating = null, StoryRating = 3 }.ToString().ShouldBe("1||3");
		[TestMethod]
		public void StoryRating_null() => new Ratings { OverallRating = 1, PerformanceRating = 2, StoryRating = null }.ToString().ShouldBe("1|2|");
		[TestMethod]
		public void populated() => new Ratings { OverallRating = 1, PerformanceRating = 2, StoryRating = 3 }.ToString().ShouldBe("1|2|3");
	}

	[TestClass]
	public class ReviewContentScores_ToString
	{
		[TestMethod]
		public void NumHelpfulVotes_null() => new ReviewContentScores { NumHelpfulVotes = null, NumUnhelpfulVotes = 9 }.ToString().ShouldBe("Helpful=0, Unhelpful=9");
		[TestMethod]
		public void NumUnhelpfulVotes_null() => new ReviewContentScores { NumHelpfulVotes = 1, NumUnhelpfulVotes = null }.ToString().ShouldBe("Helpful=1, Unhelpful=0");
		[TestMethod]
		public void populated() => new ReviewContentScores { NumHelpfulVotes = 1, NumUnhelpfulVotes = 9 }.ToString().ShouldBe("Helpful=1, Unhelpful=9");
	}

	[TestClass]
	public class Plan_ToString
	{
		[TestMethod]
		public void PlanName_null() => new Plan { PlanName = null }.ToString().ShouldBe("");
		[TestMethod]
		public void populated() => new Plan { PlanName = PlanName.Radio }.ToString().ShouldBe("Radio");
	}

	[TestClass]
	public class Price_ToString
	{
		[TestMethod]
		public void ListPrice_null() => new Price { ListPrice = null, LowestPrice = new ListPriceClass { Base = 1.2 } }.ToString().ShouldBe("List=, Lowest=1.2");
		[TestMethod]
		public void LowestPrice_null() => new Price { ListPrice = new ListPriceClass { Base = 8.9 }, LowestPrice = null }.ToString().ShouldBe("List=8.9, Lowest=");
		[TestMethod]
		public void populated() => new Price { ListPrice = new ListPriceClass { Base = 8.9 }, LowestPrice = new ListPriceClass { Base = 1.2 } }.ToString().ShouldBe("List=8.9, Lowest=1.2");
	}

	[TestClass]
	public class ListPriceClass_ToString
	{
		[TestMethod]
		public void Base_null() => new ListPriceClass { Base = null }.ToString().ShouldBe("");
		[TestMethod]
		public void populated() => new ListPriceClass { Base = 1.2 }.ToString().ShouldBe("1.2");
	}

	[TestClass]
	public class ProductImages_Fields
	{
		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);

		[TestMethod]
		public void PictureId_The500_null() => new ProductImages { The500 = null }.PictureId.ShouldBeNull();
		[TestMethod]
		public void PictureId_populated() => new ProductImages { The500 = uri }.PictureId.ShouldBe("t");
	}

	[TestClass]
	public class ProductImages_ToString
	{
		string uriPath => "http://x/t.c";
		Uri uri => new Uri(uriPath);

		[TestMethod]
		public void The500_null() => new ProductImages { The500 = null }.ToString().ShouldBe("");
		[TestMethod]
		public void populated() => new ProductImages { The500 = uri }.ToString().ShouldBe(uriPath);
	}

	[TestClass]
	public class Rating_ToString
	{
		[TestMethod]
		public void OverallDistribution_null() => new Rating { OverallDistribution = null, PerformanceDistribution = new Distribution { DisplayStars = 4.56 }, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().ShouldBe("|4.6|7.9");
		[TestMethod]
		public void PerformanceDistribution_null() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = null, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().ShouldBe("1.2||7.9");
		[TestMethod]
		public void StoryDistribution_null() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = new Distribution { DisplayStars = 4.56 }, StoryDistribution = null }.ToString().ShouldBe("1.2|4.6|");
		[TestMethod]
		public void populated() => new Rating { OverallDistribution = new Distribution { DisplayStars = 1.23 }, PerformanceDistribution = new Distribution { DisplayStars = 4.56 }, StoryDistribution = new Distribution { DisplayStars = 7.89 } }.ToString().ShouldBe("1.2|4.6|7.9");
	}

	[TestClass]
	public class Distribution_ToString
	{
		[TestMethod]
		public void DisplayStars_null() => new Distribution { DisplayStars = null }.ToString().ShouldBe("");
		[TestMethod]
		public void populated() => new Distribution { DisplayStars = 4.56 }.ToString().ShouldBe("4.6");
	}

	[TestClass]
	public class Relationship_ToString
	{
		[TestMethod]
		public void RelationshipToProduct_null() => new Relationship { RelationshipToProduct = null, RelationshipType = RelationshipType.Season }.ToString().ShouldBe(" season");
		[TestMethod]
		public void RelationshipType_null() => new Relationship { RelationshipToProduct = RelationshipToProduct.Child, RelationshipType = null }.ToString().ShouldBe("child ");
		[TestMethod]
		public void populated() => new Relationship { RelationshipToProduct = RelationshipToProduct.Child, RelationshipType = RelationshipType.Season }.ToString().ShouldBe("child season");
	}

	[TestClass]
	public class Series_ToString
	{
		[TestMethod]
		public void null_Asin() => new Series { Asin = null, Title = "t" }.ToString().ShouldBe("[] t");
		[TestMethod]
		public void null_Title() => new Series { Asin = "a", Title = null }.ToString().ShouldBe("[a] ");
		[TestMethod]
		public void populated() => new Series { Asin = "a", Title = "t" }.ToString().ShouldBe("[a] t");
	}
}
