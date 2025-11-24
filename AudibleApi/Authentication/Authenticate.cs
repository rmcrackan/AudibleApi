using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AudibleApi.Authentication
{
    internal class Authenticate
    {
        public IHttpClient LoginClient { get; }
        public ISystemDateTime SystemDateTime { get; }
        public Locale Locale { get; }
		public RegistrationOptions RegistrationOptions { get; }

		public Authenticate(Locale locale, string deviceName) : this(locale, deviceName, ApiHttpClient.Create(), new SystemDateTime())
			=> StackBlocker.ApiTestBlocker();

		public Authenticate(Locale locale, string deviceName, IHttpClient client, ISystemDateTime systemDateTime)
		{
			LoginClient = ArgumentValidator.EnsureNotNull(client, nameof(client));
			if (LoginClient.CookieJar.ReflectOverAllCookies().Count > 0)
				throw new ArgumentException("Cannot use a client which already has cookies");

			SystemDateTime = ArgumentValidator.EnsureNotNull(systemDateTime, nameof(systemDateTime));
			Locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
			RegistrationOptions = new RegistrationOptions(deviceName);

			initClientState();
		}
        private void initClientState()
        {
            var baseUri = Locale.LoginUri();

            LoginClient.Timeout = new TimeSpan(0, 0, 30);
            LoginClient.BaseAddress = baseUri;

            LoginClient.DefaultRequestHeaders.Add("Accept-Language", Locale.Language);
            LoginClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            LoginClient.DefaultRequestHeaders.Add("Host", baseUri.Host);
            LoginClient.DefaultRequestHeaders.Add("User-Agent", Resources.User_Agent);
            LoginClient.CookieJar.Add(RegistrationOptions.GetSignInCookies(Locale));
        }

        /// <summary>PUBLIC ENTRY POINT</summary>
        public static async Task<LoginResult> SubmitCredentialsAsync(Locale locale, string deviceName, string email, string password)
        {
            StackBlocker.ApiTestBlocker();

            var auth = new Authenticate(locale, deviceName);
            var loginResult = await auth.SubmitCredentialsAsync(email, password);
            return loginResult;
        }

        /// <summary>sessions-token is typically found on the 3rd trip</summary>
        public int MaxLoadSessionCookiesTrips { get; set; } = 6;
		// 1st step before calling any other page
		public async Task LoadSessionCookiesAsync()
        {
            var iterations = 0;

            // reload 'get' until session token is in cookies
            while (!LoginClient.CookieJar.EnumerateCookies(Locale.LoginUri()).Any(c => c.Name.ToLower() == "session-token"))
            {
                await LoginClient.GetAsync(Locale.LoginUri());

                iterations++;

                if (iterations > MaxLoadSessionCookiesTrips)
                    throw new TimeoutException($"{nameof(Authenticate)}.{nameof(LoadSessionCookiesAsync)} exceeded maximum attempts. {nameof(MaxLoadSessionCookiesTrips)}={MaxLoadSessionCookiesTrips}");
            }
        }

        public async Task<LoginResult> SubmitCredentialsAsync(string email, string password)
        {
            // 1st visit: response = get 1st login page
            var login1_body = await getInitialLoginPage();

            var page = new CredentialsPage(this, login1_body);
            return await page.SubmitAsync(email, password);
        }

        private async Task<string> getInitialLoginPage()
        {
            var response = await LoginClient.GetAsync(RegistrationOptions.OAuthUrl(Locale));
            response.EnsureSuccessStatusCode();

            var login1_body = await response.Content.ReadAsStringAsync();
            return login1_body;
        }
	}
}
