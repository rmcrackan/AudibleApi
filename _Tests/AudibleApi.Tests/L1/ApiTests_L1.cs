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
	static class InitializeState
    {
        public static async Task<Api> REAL_InitAndGetApi()
		{
			// goals:
			// - init with real identity tokens
			// - on initial load only, update live file tokens
			// - tests should be able to alter identity instance without worrying about affecting live token file
			await updateLiveFile();

			var identity = REAL.GetIdentity();
			var identityMaintatiner = await IdentityMaintainer.CreateAsync(identity);
			return new Api(identityMaintatiner);
		}

		private static async Task updateLiveFile()
		{
			// load live identity file into a persistent object.
			// Injecting into IdentityMaintainer runs EnsureStateAsync.
			// Expired token will be updated which will persist to life file.
			// Dispose will unsubscribe IdentityPersistent from future changes to identity
			using var idPersist = new IdentityPersistent(REAL.TokenFilePath);
			await IdentityMaintainer.CreateAsync(idPersist);
		}
	}

}

namespace ApiTests_L1.Inherited
{
    //
    // runs all inherited tests, initialized with REAL identity and Api
    //

#if !L1_ENABLED
    [Ignore]
#endif
    [TestClass]
    public class AdHocNonAuthenticatedGetAsync : ApiTests_L0.AdHocNonAuthenticatedGetAsync
	{
		public AdHocNonAuthenticatedGetAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
    [Ignore]
#endif
    [TestClass]
    public class AdHocAuthenticatedGetAsync : ApiTests_L0.AdHocAuthenticatedGetAsync
	{
		public AdHocAuthenticatedGetAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}
}
