using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	public interface IAuthorize
	{
		Task<JObject> RegisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies);

		Task<bool> DeregisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies);

		Task<AccessToken> RefreshAccessTokenAsync(RefreshToken refresh_token);

		Task<AccessToken> ExtractAccessTokenAsync(HttpResponseMessage response);
	}
}