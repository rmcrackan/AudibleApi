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

namespace ApiTests_L1.Inherited
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetLibraryAsync : ApiTests_L0.GetLibraryAsync
	{
		public GetLibraryAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetLibraryAsync_libraryOptions : ApiTests_L0.GetLibraryAsync_libraryOptions
	{
		public GetLibraryAsync_libraryOptions()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}

#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class GetLibraryBookAsync_responseGroups : ApiTests_L0.GetLibraryBookAsync_responseGroups
	{
		public GetLibraryBookAsync_responseGroups()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}
}
