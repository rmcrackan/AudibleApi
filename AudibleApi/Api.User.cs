using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public partial class Api
    {
        public async Task<JObject> UserProfileAsync()
        {
			// note: this call uses the amazon api uri, NOT audible
			var client = Sharer.GetSharedHttpClient(Resources.AmazonApiUri);

            var accessToken = await _identityMaintainer.GetAccessTokenAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/user/profile?access_token={accessToken.TokenValue}");
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            // return full json string. consumer to parse it
            return JObject.Parse(json);
        }
    }
}
