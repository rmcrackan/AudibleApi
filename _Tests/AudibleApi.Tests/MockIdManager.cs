using System;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using TestCommon;
using static AuthorizationShared.Shared;
using static AuthorizationShared.Shared.AccessTokenTemporality;

namespace TestAudibleApiCommon
{
    public class MockIdMaintainer : IdentityMaintainer
	{
		public MockIdMaintainer() : this(GetIdentity(Future), HttpMock.GetHandler()) { }
		public MockIdMaintainer(IIdentity identity, HttpMessageHandler handler)
			: base(
			identity,
			new Authorize(
				Locale.Empty,
				new HttpClientSharer(handler),
				StaticSystemDateTime.Past),
			StaticSystemDateTime.Past)
		{ }

		public new Task RegisterAsync() => base.RegisterAsync();
        public new Task DeregisterAsync() => base.DeregisterAsync();
        public new Task RefreshAccessTokenAsync() => base.RefreshAccessTokenAsync();
		public new Task EnsureStateAsync() => base.EnsureStateAsync();
	}
}
