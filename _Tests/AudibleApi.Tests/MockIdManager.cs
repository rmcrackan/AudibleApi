using System;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using static AuthorizationShared.Shared;

namespace TestAudibleApiCommon
{
    public class MockIdMaintainer : IdentityMaintainer
	{
		public MockIdMaintainer() : this(GetIdentity_Future(), ApiClientMock.GetHandler()) { }
		public MockIdMaintainer(IIdentity identity, HttpMessageHandler handler)
			: base(
			identity,
			new Authorize(
				new ClientSharer(handler),
				StaticSystemDateTime.Past),
			StaticSystemDateTime.Past)
		{ }

		public new Task RegisterAsync() => base.RegisterAsync();
        public new Task DeregisterAsync() => base.DeregisterAsync();
        public new Task RefreshAccessTokenAsync() => base.RefreshAccessTokenAsync();
		public new Task EnsureStateAsync() => base.EnsureStateAsync();
	}
}
