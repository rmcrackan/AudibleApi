namespace IEnumerable_Item_ExtensionsTests
{
	[TestClass]
	public class GetAuthorsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetAuthorsDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetAuthorsDistinct().Count().ShouldBe(0);

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
			items.GetAuthorsDistinct().Count().ShouldBe(0);
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
			items.GetAuthorsDistinct().Count().ShouldBe(0);
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
			d.Count.ShouldBe(1);
			d[0].Name.ShouldBe("abc");
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
			d.Count.ShouldBe(3);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
			d[2].Name.ShouldBe("foo");
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
			d.Count.ShouldBe(3);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
			d[2].Name.ShouldBe("foo");
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
			d.Count.ShouldBe(2);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
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
			d.Count.ShouldBe(2);
			d[0].Name.ShouldBe("abc");
			d[0].Asin.ShouldBe("asin1");
			d[1].Name.ShouldBe("xyz");
			d[1].Asin.ShouldBe("2");
		}
	}

	[TestClass]
	public class GetNarratorsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetNarratorsDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetNarratorsDistinct().Count().ShouldBe(0);

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
			items.GetNarratorsDistinct().Count().ShouldBe(0);
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
			items.GetNarratorsDistinct().Count().ShouldBe(0);
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
			d.Count.ShouldBe(1);
			d[0].Name.ShouldBe("abc");
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
			d.Count.ShouldBe(3);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
			d[2].Name.ShouldBe("foo");
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
			d.Count.ShouldBe(3);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
			d[2].Name.ShouldBe("foo");
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
			d.Count.ShouldBe(2);
			d[0].Name.ShouldBe("abc");
			d[1].Name.ShouldBe("xyz");
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
			d.Count.ShouldBe(2);
			d[0].Name.ShouldBe("abc");
			d[0].Asin.ShouldBe("asin1");
			d[1].Name.ShouldBe("xyz");
			d[1].Asin.ShouldBe("2");
		}
	}

	[TestClass]
	public class GetNarratorNamesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetNarratorNamesDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetNarratorNamesDistinct().Count().ShouldBe(0);

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
			items.GetNarratorNamesDistinct().Count().ShouldBe(0);
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
			items.GetNarratorNamesDistinct().Count().ShouldBe(0);
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
			d.Count.ShouldBe(1);
			d[0].ShouldBe("abc");
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
			d.Count.ShouldBe(3);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
			d[2].ShouldBe("foo");
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
			d.Count.ShouldBe(3);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
			d[2].ShouldBe("foo");
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
			d.Count.ShouldBe(2);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
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
			d.Count.ShouldBe(2);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
		}
	}

	[TestClass]
	public class GetPublishersDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetPublishersDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetPublishersDistinct().Count().ShouldBe(0);

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
			items.GetPublishersDistinct().Count().ShouldBe(0);
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
			items.GetPublishersDistinct().Count().ShouldBe(0);
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
			d.Count.ShouldBe(1);
			d[0].ShouldBe("abc");
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
			d.Count.ShouldBe(3);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
			d[2].ShouldBe("foo");
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
			d.Count.ShouldBe(2);
			d[0].ShouldBe("abc");
			d[1].ShouldBe("xyz");
		}
	}

	[TestClass]
	public class GetSeriesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetSeriesDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetSeriesDistinct().Count().ShouldBe(0);

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
			items.GetSeriesDistinct().Count().ShouldBe(0);
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
			items.GetSeriesDistinct().Count().ShouldBe(0);
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
			d.Count.ShouldBe(1);
			d[0].Title.ShouldBe("abc");
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
			d.Count.ShouldBe(3);
			d[0].Title.ShouldBe("abc");
			d[1].Title.ShouldBe("xyz");
			d[2].Title.ShouldBe("foo");
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
			d.Count.ShouldBe(3);
			d[0].Title.ShouldBe("abc");
			d[1].Title.ShouldBe("xyz");
			d[2].Title.ShouldBe("foo");
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
			d.Count.ShouldBe(2);
			d[0].Title.ShouldBe("abc");
			d[1].Title.ShouldBe("xyz");
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
			d.Count.ShouldBe(2);
			d[0].Title.ShouldBe("abc");
			d[0].Asin.ShouldBe("asin1");
			d[1].Title.ShouldBe("xyz");
			d[1].Asin.ShouldBe("2");
		}
	}

	[TestClass]
	public class GetCategoryPairsDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetCategoryPairsDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetCategoryPairsDistinct().Count().ShouldBe(0);

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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
		}

		[TestMethod]
		public void CategoryLadder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = [null]
				}
			};
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
							Ladder = [null]
						}
					}
				}
			};
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			items.GetCategoryPairsDistinct().Count().ShouldBe(0);
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
			ladderPairs.Count.ShouldBe(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.ShouldBe(1);
			var ladder = ladderPair[0];
			ladder.ShouldNotBeNull();
			ladder.Name.ShouldBe("name1");
			ladder.Id.ShouldBe("id1");
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
			ladderPairs.Count.ShouldBe(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.ShouldBe(1);
			var ladder = ladderPair[0];
			ladder.ShouldNotBeNull();
			ladder.Name.ShouldBe("name1");
			ladder.Id.ShouldBe("id1");
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
			ladderPairs.Count.ShouldBe(1);
			var ladderPair = ladderPairs[0];
			ladderPair.Length.ShouldBe(2);
			var ladder1 = ladderPair[0];
			ladder1.ShouldNotBeNull();
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladderPair[1];
			ladder2.ShouldNotBeNull();
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
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
			ladderPairs.Count.ShouldBe(2);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.ShouldBe(2);
			var ladder1 = ladderPair1[0];
			ladder1.ShouldNotBeNull();
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladderPair1[1];
			ladder2.ShouldNotBeNull();
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");

			var ladderPair2 = ladderPairs[1];
			ladderPair2.Length.ShouldBe(2);
			var ladder3 = ladderPair2[0];
			ladder3.ShouldNotBeNull();
			ladder3.Name.ShouldBe("name3");
			ladder3.Id.ShouldBe("id3");
			var ladder4 = ladderPair2[1];
			ladder4.ShouldNotBeNull();
			ladder4.Name.ShouldBe("name4");
			ladder4.Id.ShouldBe("id4");
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
			ladderPairs.Count.ShouldBe(2);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.ShouldBe(2);
			var ladder1 = ladderPair1[0];
			ladder1.ShouldNotBeNull();
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladderPair1[1];
			ladder2.ShouldNotBeNull();
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");

			var ladderPair2 = ladderPairs[1];
			ladderPair2.Length.ShouldBe(1);
			var ladder3 = ladderPair2[0];
			ladder3.ShouldNotBeNull();
			ladder3.Name.ShouldBe("name1");
			ladder3.Id.ShouldBe("id1");
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
			ladderPairs.Count.ShouldBe(1);

			var ladderPair1 = ladderPairs[0];
			ladderPair1.Length.ShouldBe(2);
			var ladder1 = ladderPair1[0];
			ladder1.ShouldNotBeNull();
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladderPair1[1];
			ladder2.ShouldNotBeNull();
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
		}
	}

	[TestClass]
	public class GetCategoriesDistinct
	{
		[TestMethod]
		public void null_list()
			=> ((List<Item>?)null).GetCategoriesDistinct().ShouldBeNull();

		[TestMethod]
		public void empty_list()
			=> new List<Item>().GetCategoriesDistinct().Count().ShouldBe(0);

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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
		}

		[TestMethod]
		public void CategoryLadder_null()
		{
			var items = new List<Item>
			{
				new Item
				{
					CategoryLadders = [null]
				}
			};
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
							Ladder = [null]
						}
					}
				}
			};
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			items.GetCategoriesDistinct().Count().ShouldBe(0);
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
			ladders.Count.ShouldBe(1);
			var ladder = ladders[0];
			ladder.Name.ShouldBe("name1");
			ladder.Id.ShouldBe("id1");
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
			ladders.Count.ShouldBe(1);
			var ladder = ladders[0];
			ladder.Name.ShouldBe("name1");
			ladder.Id.ShouldBe("id1");
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
			ladders.Count.ShouldBe(2);
			var ladder1 = ladders[0];
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladders[1];
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
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
			ladderPairs.Count.ShouldBe(4);

			var ladder1 = ladderPairs[0];
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladderPairs[1];
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
			var ladder3 = ladderPairs[2];
			ladder3.Name.ShouldBe("name3");
			ladder3.Id.ShouldBe("id3");
			var ladder4 = ladderPairs[3];
			ladder4.Name.ShouldBe("name4");
			ladder4.Id.ShouldBe("id4");
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
			ladders.Count.ShouldBe(2);

			var ladder1 = ladders[0];
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladders[1];
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
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
			ladders.Count.ShouldBe(2);

			var ladder1 = ladders[0];
			ladder1.Name.ShouldBe("name1");
			ladder1.Id.ShouldBe("id1");
			var ladder2 = ladders[1];
			ladder2.Name.ShouldBe("name2");
			ladder2.Id.ShouldBe("id2");
		}
	}
}
