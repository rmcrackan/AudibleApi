using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using BaseLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AuthorizationShared.Shared;

namespace TestAudibleApiCommon
{
    public class MockIdMaintainer : IdentityMaintainer
	{
		public MockIdMaintainer() : this(GetIdentity(), ApiClientMock.GetHandler()) { }
		public MockIdMaintainer(IIdentity identity, HttpMessageHandler handler)
			: base(
			identity,
			new Authorize(
				new ClientSharer(handler),
				StaticSystemDateTime.ByYear(2000)),
			StaticSystemDateTime.ByYear(2000))
		{ }

		public new void Parse(JObject authRegister) => base.Parse(authRegister);
        public new Task RegisterAsync() => base.RegisterAsync();
        public new Task DeregisterAsync() => base.DeregisterAsync();
        public new Task RefreshAccessTokenAsync() => base.RefreshAccessTokenAsync();
		public new Task EnsureStateAsync() => base.EnsureStateAsync();
	}
}
