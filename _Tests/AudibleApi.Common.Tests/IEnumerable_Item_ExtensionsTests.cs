namespace IEnumerable_Item_ExtensionsTests
{
	[TestClass]
	public class GetAuthorsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetAuthorsDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetAuthorsDistinct().Count().Should().Be(0);

		[TestMethod]
		public void authors_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = null
				}
			};
			items.GetAuthorsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void authors_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[] { }
				}
			};
			items.GetAuthorsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void _1_item_1_author()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc" }
					}
				}
			};
			var d = items.GetAuthorsDistinct().ToList();
			d.Count.Should().Be(1);
			d[0].Name.Should().Be("abc");
		}

		[TestMethod]
		public void _1_item_3_authors()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				}
			};
			var d = items.GetAuthorsDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
			d[2].Name.Should().Be("foo");
		}

		[TestMethod]
		public void _2_items_3_authors()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				},
			};
			var d = items.GetAuthorsDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
			d[2].Name.Should().Be("foo");
		}

		[TestMethod]
		public void duplicate_names()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" }
					}
				},
			};
			var d = items.GetAuthorsDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
		}

		[TestMethod]
		public void duplicate_names_and_ids()
		{
			var items = new List<Item>
			{
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" }
					}
				},
				new Item
				{
					Authors = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" },
						new Person { Name = "xyz", Asin = "2" }
					}
				},
			};
			var d = items.GetAuthorsDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Name.Should().Be("abc");
			d[0].Asin.Should().Be("asin1");
			d[1].Name.Should().Be("xyz");
			d[1].Asin.Should().Be("2");
		}
	}

	[TestClass]
	public class GetNarratorsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetNarratorsDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetNarratorsDistinct().Count().Should().Be(0);

		[TestMethod]
		public void narrators_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = null
				}
			};
			items.GetNarratorsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void narrators_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[] { }
				}
			};
			items.GetNarratorsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void _1_item_1_narrator()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				}
			};
			var d = items.GetNarratorsDistinct().ToList();
			d.Count.Should().Be(1);
			d[0].Name.Should().Be("abc");
		}

		[TestMethod]
		public void _1_item_3_narrators()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				}
			};
			var d = items.GetNarratorsDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
			d[2].Name.Should().Be("foo");
		}

		[TestMethod]
		public void _2_items_3_narrators()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				},
			};
			var d = items.GetNarratorsDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
			d[2].Name.Should().Be("foo");
		}

		[TestMethod]
		public void duplicate_names()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" }
					}
				},
			};
			var d = items.GetNarratorsDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Name.Should().Be("abc");
			d[1].Name.Should().Be("xyz");
		}

		[TestMethod]
		public void duplicate_names_and_ids()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" },
						new Person { Name = "xyz", Asin = "2" }
					}
				},
			};
			var d = items.GetNarratorsDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Name.Should().Be("abc");
			d[0].Asin.Should().Be("asin1");
			d[1].Name.Should().Be("xyz");
			d[1].Asin.Should().Be("2");
		}
	}

	[TestClass]
	public class GetNarratorNamesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetNarratorNamesDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetNarratorNamesDistinct().Count().Should().Be(0);

		[TestMethod]
		public void narrators_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = null
				}
			};
			items.GetNarratorNamesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void narrators_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[] { }
				}
			};
			items.GetNarratorNamesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void _1_item_1_narrator()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				}
			};
			var d = items.GetNarratorNamesDistinct().ToList();
			d.Count.Should().Be(1);
			d[0].Should().Be("abc");
		}

		[TestMethod]
		public void _1_item_3_narrators()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				}
			};
			var d = items.GetNarratorNamesDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
			d[2].Should().Be("foo");
		}

		[TestMethod]
		public void _2_items_3_narrators()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "xyz" },
						new Person { Name = "foo" }
					}
				},
			};
			var d = items.GetNarratorNamesDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
			d[2].Should().Be("foo");
		}

		[TestMethod]
		public void duplicate_names()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc" },
						new Person { Name = "xyz" }
					}
				},
			};
			var d = items.GetNarratorNamesDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
		}

		[TestMethod]
		public void duplicate_names_and_ids()
		{
			var items = new List<Item>
			{
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" }
					}
				},
				new Item
				{
					Narrators = new Person[]
					{
						new Person { Name = "abc", Asin = "asin1" },
						new Person { Name = "xyz", Asin = "2" }
					}
				},
			};
			var d = items.GetNarratorNamesDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
		}
	}

	[TestClass]
	public class GetPublishersDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetPublishersDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetPublishersDistinct().Count().Should().Be(0);

		[TestMethod]
		public void publishers_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					PublisherName = null
				}
			};
			items.GetPublishersDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void publishers_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					PublisherName = ""
				}
			};
			items.GetPublishersDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void one_item()
		{
			var items = new List<Item>
			{
				new Item
				{
					PublisherName = "abc"
				}
			};
			var d = items.GetPublishersDistinct().ToList();
			d.Count.Should().Be(1);
			d[0].Should().Be("abc");
		}

		[TestMethod]
		public void multiple_items()
		{
			var items = new List<Item>
			{
				new Item { PublisherName = "abc" },
				new Item { PublisherName = "xyz" },
				new Item { PublisherName = "foo" }
			};
			var d = items.GetPublishersDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
			d[2].Should().Be("foo");
		}

		[TestMethod]
		public void duplicate_names()
		{
			var items = new List<Item>
			{
				new Item { PublisherName = "abc" },
				new Item { PublisherName = "abc" },
				new Item { PublisherName = "xyz" }
			};
			var d = items.GetPublishersDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Should().Be("abc");
			d[1].Should().Be("xyz");
		}
	}

	[TestClass]
	public class GetSeriesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetSeriesDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetSeriesDistinct().Count().Should().Be(0);

		[TestMethod]
		public void series_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = null
				}
			};
			items.GetSeriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void series_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]{ }
				}
			};
			items.GetSeriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void _1_item_1_series()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc" }
					}
				}
			};
			var d = items.GetSeriesDistinct().ToList();
			d.Count.Should().Be(1);
			d[0].Title.Should().Be("abc");
		}

		[TestMethod]
		public void _1_item_3_series()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc" },
						new Series { Title = "xyz" },
						new Series { Title = "foo" }
					}
				}
			};
			var d = items.GetSeriesDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Title.Should().Be("abc");
			d[1].Title.Should().Be("xyz");
			d[2].Title.Should().Be("foo");
		}

		[TestMethod]
		public void _2_items_3_series()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc" }
					}
				},
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "xyz" },
						new Series { Title = "foo" }
					}
				},
			};
			var d = items.GetSeriesDistinct().ToList();
			d.Count.Should().Be(3);
			d[0].Title.Should().Be("abc");
			d[1].Title.Should().Be("xyz");
			d[2].Title.Should().Be("foo");
		}

		[TestMethod]
		public void duplicate_names()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc" }
					}
				},
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc" },
						new Series { Title = "xyz" }
					}
				},
			};
			var d = items.GetSeriesDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Title.Should().Be("abc");
			d[1].Title.Should().Be("xyz");
		}

		[TestMethod]
		public void duplicate_names_and_ids()
		{
			var items = new List<Item>
			{
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc", Asin = "asin1" }
					}
				},
				new Item
				{
					Series = new Series[]
					{
						new Series { Title = "abc", Asin = "asin1" },
						new Series { Title = "xyz", Asin = "2" }
					}
				},
			};
			var d = items.GetSeriesDistinct().ToList();
			d.Count.Should().Be(2);
			d[0].Title.Should().Be("abc");
			d[0].Asin.Should().Be("asin1");
			d[1].Title.Should().Be("xyz");
			d[1].Asin.Should().Be("2");
		}
	}

	[TestClass]
	public class GetCategoryPairsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetCategoryPairsDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetCategoryPairsDistinct().Count().Should().Be(0);

		[TestMethod]
		public void CategoryLadder_set_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = null
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_set_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[0]
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						null
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder()
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_set_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = null
						}
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_set_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[0]
						}
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								null
							}
						}
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder()
							}
						}
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void _1_Ladder()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.Should().Be(1);
			var ladder = ladderPair[0];
			ladder.Name.Should().Be("name1");
			ladder.Id.Should().Be("id1");
		}

		[TestMethod]
		public void ignore_2nd_CategoryLadder()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						},
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "error", Id = "ex" }
							}
						},
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.Should().Be(1);
			var ladder = ladderPair[0];
			ladder.Name.Should().Be("name1");
			ladder.Id.Should().Be("id1");
		}

		[TestMethod]
		public void _1_Ladder_pair()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.Should().Be(2);
			var ladder1 = ladderPair[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladderPair[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
		}

		[TestMethod]
		public void _2_items()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name3", Id = "id3" },
								new Ladder { Name = "name4", Id = "id4" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(2);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.Should().Be(2);
			var ladder1 = ladderPair1[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladderPair1[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");

			var ladderPair2 = ladderPairs[1];
			ladderPair2.Length.Should().Be(2);
			var ladder3 = ladderPair2[0];
			ladder3.Name.Should().Be("name3");
			ladder3.Id.Should().Be("id3");
			var ladder4 = ladderPair2[1];
			ladder4.Name.Should().Be("name4");
			ladder4.Id.Should().Be("id4");
		}

		[TestMethod]
		public void different_child()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(2);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.Should().Be(2);
			var ladder1 = ladderPair1[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladderPair1[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");

			var ladderPair2 = ladderPairs[1];
			ladderPair2.Length.Should().Be(1);
			var ladder3 = ladderPair2[0];
			ladder3.Name.Should().Be("name1");
			ladder3.Id.Should().Be("id1");
		}

		[TestMethod]
		public void duplicate_parent_and_child()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoryPairsDistinct().ToList();
			ladderPairs.Count.Should().Be(1);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.Should().Be(2);
			var ladder1 = ladderPair1[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladderPair1[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
		}
	}

	[TestClass]
	public class GetCategoriesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>)null).GetCategoriesDistinct().Should().BeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetCategoriesDistinct().Count().Should().Be(0);

		[TestMethod]
		public void CategoryLadder_set_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = null
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_set_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[0]
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						null
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void CategoryLadder_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder()
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_set_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = null
						}
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_set_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[0]
						}
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								null
							}
						}
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void Ladder_empty()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder()
							}
						}
					}
				}
			};
			items.GetCategoriesDistinct().Count().Should().Be(0);
		}

		[TestMethod]
		public void ignore_2nd_CategoryLadder()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						},
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "error", Id = "ex" }
							}
						},
					}
				}
			};
			var ladders = items.GetCategoriesDistinct().ToList();
			ladders.Count.Should().Be(1);
			var ladder = ladders[0];
			ladder.Name.Should().Be("name1");
			ladder.Id.Should().Be("id1");
		}

		[TestMethod]
		public void _1_item_1_Ladder()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						}
					}
				}
			};
			var ladders = items.GetCategoriesDistinct().ToList();
			ladders.Count.Should().Be(1);
			var ladder = ladders[0];
			ladder.Name.Should().Be("name1");
			ladder.Id.Should().Be("id1");
		}

		[TestMethod]
		public void _1_item_2_Ladders()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				}
			};
			var ladders = items.GetCategoriesDistinct().ToList();
			ladders.Count.Should().Be(2);
			var ladder1 = ladders[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladders[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
		}

		[TestMethod]
		public void _2_items_4_Ladders()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name3", Id = "id3" },
								new Ladder { Name = "name4", Id = "id4" }
							}
						}
					}
				}
			};
			var ladderPairs = items.GetCategoriesDistinct().ToList();
			ladderPairs.Count.Should().Be(4);

			var ladder1 = ladderPairs[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladderPairs[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
			var ladder3 = ladderPairs[2];
			ladder3.Name.Should().Be("name3");
			ladder3.Id.Should().Be("id3");
			var ladder4 = ladderPairs[3];
			ladder4.Name.Should().Be("name4");
			ladder4.Id.Should().Be("id4");
		}

		[TestMethod]
		public void duplicate_parent()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" }
							}
						}
					}
				}
			};
			var ladders = items.GetCategoriesDistinct().ToList();
			ladders.Count.Should().Be(2);

			var ladder1 = ladders[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladders[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
		}

		[TestMethod]
		public void duplicate_parent_and_child()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				},
				new Item
				{
					CategoryLadders = new CategoryLadder[]
					{
						new CategoryLadder
						{
							Ladder = new Ladder[]
							{
								new Ladder { Name = "name1", Id = "id1" },
								new Ladder { Name = "name2", Id = "id2" }
							}
						}
					}
				}
			};
			var ladders = items.GetCategoriesDistinct().ToList();
			ladders.Count.Should().Be(2);

			var ladder1 = ladders[0];
			ladder1.Name.Should().Be("name1");
			ladder1.Id.Should().Be("id1");
			var ladder2 = ladders[1];
			ladder2.Name.Should().Be("name2");
			ladder2.Id.Should().Be("id2");
		}
	}
}
