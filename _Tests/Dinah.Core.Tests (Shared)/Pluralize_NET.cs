using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PluralizerTests
{
	[TestClass]
	public class Pluralize_NET
	{
		public static Dictionary<string, string> Dictionary { get; } = new Dictionary<string, string>
		{
			["toe"] = "toes",
			["shoe"] = "shoes",
			["Entity"] = "Entities",
			["PERSON"] = "PEOPLE",
			["fish"] = "fish",
			["deer"] = "deer",
			["sheep"] = "sheep",
			["house"] = "houses",
			["mouse"] = "mice",
			["louse"] = "lice",
			["Box"] = "Boxes",
			["index"] = "indices",
			["OCTOPUS"] = "OCTOPI",
			["man"] = "men",
			["woman"] = "women"
		};

		[TestMethod]
		public void Test_Pluralize_NET()
		{
			foreach (var kvp in Dictionary)
			{
				var sing = kvp.Key;
				var pl = kvp.Value;

				var p = new Pluralize.NET.Pluralizer();

				p.Singularize(sing).Should().Be(sing);
				p.Singularize(pl).Should().Be(sing);

				p.Pluralize(sing).Should().Be(pl);
				p.Pluralize(pl).Should().Be(pl);

				p.IsSingular(sing).Should().BeTrue();
				p.IsPlural(pl).Should().BeTrue();

				if (sing != pl)
				{
					p.IsSingular(pl).Should().BeFalse();
					p.IsPlural(sing).Should().BeFalse();
				}

				p.Format(sing, 0).Should().Be(pl);
				p.Format(sing, 1).Should().Be(sing);
				p.Format(sing, 5).Should().Be(pl);
				p.Format(pl, 0).Should().Be(pl);
				p.Format(pl, 1).Should().Be(sing);
				p.Format(pl, 5).Should().Be(pl);

				p.Format(sing, 0, true).Should().Be("0 " + pl);
				p.Format(sing, 1, true).Should().Be("1 " + sing);
				p.Format(sing, 5, true).Should().Be("5 " + pl);
				p.Format(pl, 0, true).Should().Be("0 " + pl);
				p.Format(pl, 1, true).Should().Be("1 " + sing);
				p.Format(pl, 5, true).Should().Be("5 " + pl);
			}
		}
	}
}
