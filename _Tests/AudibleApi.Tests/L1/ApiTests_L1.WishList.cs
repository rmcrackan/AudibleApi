//// uncomment to run these tests
//#define L1_ENABLED

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
	public class WishList
	{
		[TestMethod]
		public async Task all_live_wishlist_calls()
		{
			var api = await InitializeState.REAL_InitAndGetApi();

			var asin = "172137406X";

			// verify not in list
			(await api.IsInWishListAsync(asin)).Should().BeFalse();

			// adds
			await api.AddToWishListAsync(asin);
			(await api.IsInWishListAsync(asin)).Should().BeTrue();

			// attempt to add again: no effect
			await api.AddToWishListAsync(asin);
			(await api.IsInWishListAsync(asin)).Should().BeTrue();

			// delete
			await api.DeleteFromWishListAsync(asin);
			(await api.IsInWishListAsync(asin)).Should().BeFalse();

			// attempt to delete again: no effect
			await api.DeleteFromWishListAsync(asin);
			(await api.IsInWishListAsync(asin)).Should().BeFalse();
		}
	}
}

namespace ApiTests_L1.Inherited
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class IsInWishListAsync : ApiTests_L0.IsInWishListAsync
	{
		public IsInWishListAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class AddToWishListAsync : ApiTests_L0.AddToWishListAsync
	{
		public AddToWishListAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class DeleteFromWishListAsync : ApiTests_L0.DeleteFromWishListAsync
	{
		public DeleteFromWishListAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}
}
