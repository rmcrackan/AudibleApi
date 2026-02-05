using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi.Authorization;

public interface IAuthorize
{
	Task<JObject> RegisterAsync(OAuth2 authorization);

	Task<bool> DeregisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string?>>? cookies);

	Task<AccessToken> RefreshAccessTokenAsync(RefreshToken refresh_token);

	Task<AccessToken> ExtractAccessTokenAsync(HttpResponseMessage response);
}