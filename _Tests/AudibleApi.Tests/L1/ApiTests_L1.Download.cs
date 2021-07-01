//// uncomment to run these tests
//#define L1_ENABLED
//#define RUN_LIVE_DOWNLOADS
//#define RUN_LIVE_HUGE_DOWNLOADS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace ApiTests_L1
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetDownloadLicenseAsync
	{
		[TestMethod]
		public async Task get_harry_potter_link()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var license = await api.GetDownloadLicenseAsync("B017V4IM1G");
			license.Should().NotBeNull();
			license.ContentMetadata.Should().NotBeNull();
			license.ContentMetadata.ContentUrl.Should().NotBeNull();
			license.ContentMetadata.ContentUrl.OfflineUrl.Should().NotBeNull();

			var link = license.ContentMetadata.ContentUrl.OfflineUrl;

			link.Should().Contain("cloudfront.net");
			link.Should().Contain("/bk_potr_000001");
			link.Should().Contain(".aax");
			link.Should().Contain("?voucherId=");
			link.Should().Contain("&Expires=");
			link.Should().Contain("&Signature=");
			link.Should().Contain("&Key-Pair-Id=");
		}
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetDownloadablePartsAsync
	{
		[TestMethod]
		public async Task _1_part()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var harryPotterAsin = "B017V4IM1G";
			var expected = new List<string> { harryPotterAsin };

			var list = await api.GetDownloadablePartsAsync(harryPotterAsin);
			list.Should().Equal(expected);
		}

		[TestMethod]
		public async Task _6_parts()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var sherlockHolmesAsin = "B06WLMWF2S";
			var expected = new List<string> { "B06WWG59CP", "B06WVKGR8M", "B06WW37874", "B06WW57B11", "B06WVVVBW5", "B06WVVVC7T" };

			var list = await api.GetDownloadablePartsAsync(sherlockHolmesAsin);
			list.Should().Equal(expected);
		}
	}

#if !L1_ENABLED || !RUN_LIVE_DOWNLOADS
	[Ignore]
#endif
	[TestClass]
	public class DownloadPartAsync
	{
		// this is just a proof of concept/L1 test.
		// should always use DownloadAsync NOT DownloadPartAsync
		[TestMethod]
		public async Task single_part_book_success()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var mimisAdventureAsin = "B079DZ8YMP";

			// this wrong extension will be corrected by GetDownloadFilePath
			var file = mimisAdventureAsin + ".txt";
			var expected = mimisAdventureAsin + ".aax";

			var final = await api.DownloadPartAsync(mimisAdventureAsin, file);
			final.Should().Be(expected);
			File.Exists(expected).Should().BeTrue();

			await Task.Delay(100);
			File.Delete(final);
		}

		// actually an error in GetDownloadLinkAsync. see its unit tests
		// public async Task multi_part_book_failure()
	}

#if !L1_ENABLED || !RUN_LIVE_HUGE_DOWNLOADS
	[Ignore]
#endif
	[TestClass]
	public class DownloadAsync
	{
		[TestMethod]
		public async Task download_1_part_book()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var mimisAdventureAsin = "B079DZ8YMP";

			var expected = "_mimisAdventure.aax";

			try
			{
				var files = (await api.DownloadAsync(mimisAdventureAsin, "_mimisAdventure.aax")).ToList();

				files.Count.Should().Be(1);
				files[0].Should().Be(expected);

				File.Exists(expected).Should().BeTrue();
			}
			finally
			{
				await Task.Delay(100);
				File.Delete(expected);
			}
		}

		[TestMethod]
		public async Task download_multi_part_book()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var sherlockHolmesAsin = "B06WLMWF2S";

			var expected = new List<string>
			{
				"_sherlockHolmes(1).aax",
				"_sherlockHolmes(2).aax",
				"_sherlockHolmes(3).aax",
				"_sherlockHolmes(4).aax",
				"_sherlockHolmes(5).aax",
				"_sherlockHolmes(6).aax"
			};

			try
			{
				var files = (await api.DownloadAsync(sherlockHolmesAsin, "_sherlockHolmes.aax")).ToList();

				files.Count.Should().Be(6);
				files.Should().Equal(expected);

				foreach (var file in expected)
					File.Exists(file).Should().BeTrue();
			}
			finally
			{
				await Task.Delay(100);

				foreach (var file in expected)
					File.Delete(file);
			}
		}
	}

#if !L1_ENABLED || !RUN_LIVE_HUGE_DOWNLOADS
	[Ignore]
#endif
	[TestClass]
	public class DownloadAaxWorkaroundAsync
	{
		[TestMethod]
		public async Task download_tiny_file()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var mimisAdventure = "B079DZ8YMP";
			var expected = $"_{mimisAdventure}.aax";

			try
			{
				var file = await api.DownloadAaxWorkaroundAsync(mimisAdventure, expected);
				file.Should().Be(expected);
				File.Exists(expected).Should().BeTrue();
			}
			finally
			{
				await Task.Delay(100);
				File.Delete(expected);
			}
		}

		[TestMethod]
		public async Task download_medium_file()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var harryPotter = "B017V4IM1G";
			var expected = $"_{harryPotter}.aax";

			try
			{
				var file = await api.DownloadAaxWorkaroundAsync(harryPotter, expected);
				file.Should().Be(expected);
				File.Exists(expected).Should().BeTrue();
			}
			finally
			{
				await Task.Delay(100);
				File.Delete(expected);
			}
		}

		[TestMethod]
		public async Task download_huge_file_as_single_file()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var sherlockHolmes = "B06WLMWF2S";
			var expected = $"_{sherlockHolmes}.aax";

			try
			{
				var file = await api.DownloadAaxWorkaroundAsync(sherlockHolmes, expected);
				file.Should().Be(expected);
				File.Exists(expected).Should().BeTrue();
			}
			finally
			{
				await Task.Delay(100);
				File.Delete(expected);
			}
		}
	}
}

namespace ApiTests_L1.Inherited
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetDownloadLicenseAsync : ApiTests_L0.GetDownloadLicenseAsync
	{
		public GetDownloadLicenseAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class DownloadPartAsync : ApiTests_L0.DownloadPartAsync
	{
		public DownloadPartAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetDownloadablePartsAsync : ApiTests_L0.GetDownloadablePartsAsync
	{
		public GetDownloadablePartsAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class DownloadAsync : ApiTests_L0.DownloadAsync
	{
		public DownloadAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class DownloadAaxWorkaroundAsync : ApiTests_L0.DownloadAaxWorkaroundAsync
	{
		public DownloadAaxWorkaroundAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}
}
