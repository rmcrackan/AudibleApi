using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EzApiCreatorTests_L0
{
	[TestClass]
	public class GetApiAsync
	{
		[TestMethod]
		public async Task access_from_L0_throws()
			=> await Assert.ThrowsExceptionAsync<MethodAccessException>(() => EzApiCreator.GetApiAsync(null));
	}
}

namespace EzApiCreatorTests_L1
{
}
