using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public partial class Api
	{
		/// <summary>Get email from: /user/profile</summary>
		public async Task<string> GetEmailAsync()
		{
			var u = await UserProfileAsync();
			var email = u["email"].Value<string>();
			return email;
		}

		public async Task<JObject> UserProfileAsync()
		{
			// note: this call uses the amazon api uri, NOT audible
			var client = Sharer.GetSharedHttpClient(Locale.AmazonApiUri());

			var response = await AdHocAuthenticatedGetWithAccessTokenAsync($"/user/profile", client);

			var json = await response.Content.ReadAsStringAsync();

			// return full json string. consumer to parse it
			return JObject.Parse(json);
		}
	}
}
