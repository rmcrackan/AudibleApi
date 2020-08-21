using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public class Authenticate
    {
		public CookieCollection GetCookies(Uri uri) => loginClient.CookieJar.GetCookies(uri);

		private IHttpClient loginClient { get; }

		private ISystemDateTime _systemDateTime { get; }
        private Locale _locale { get; }

        public Authenticate(Locale locale) : this(locale, ApiHttpClient.Create(), new SystemDateTime())
			=> StackBlocker.ApiTestBlocker();

		public Authenticate(Locale locale, IHttpClient client, ISystemDateTime systemDateTime)
		{
			loginClient = client ?? throw new ArgumentNullException(nameof(client));
			if (loginClient.CookieJar.ReflectOverAllCookies().Count > 0)
				throw new ArgumentException("Cannot use a client which already has cookies");

            _systemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
            _locale = locale ?? throw new ArgumentNullException(nameof(locale));

            initClientState();
        }
		private void initClientState()
		{
            var baseUri = _locale.AmazonLoginUri();

            loginClient.Timeout = new TimeSpan(0, 0, 30);
			loginClient.BaseAddress = baseUri;

            loginClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            loginClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            loginClient.DefaultRequestHeaders.Add("Accept-Language", _locale.LanguageTag());
            loginClient.DefaultRequestHeaders.Add("Host", baseUri.Host);
            loginClient.DefaultRequestHeaders.Add("Origin", baseUri.GetOrigin());
            loginClient.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);
        }

        /// <summary>sessions-token is typically found on the 3rd trip</summary>
        public int MaxLoadSessionCookiesTrips { get; set; } = 6;
		// 1st step before calling any other page
		public async Task LoadSessionCookiesAsync()
        {
            var iterations = 0;

            // reload 'get' until session token is in cookies
            while (!loginClient.CookieJar.EnumerateCookies(_locale.AmazonLoginUri()).Any(c => c.Name.ToLower() == "session-token"))
            {
                await loginClient.GetAsync(_locale.AmazonLoginUri());

                iterations++;

                if (iterations > MaxLoadSessionCookiesTrips)
                    throw new TimeoutException($"{nameof(Authenticate)}.{nameof(LoadSessionCookiesAsync)} exceeded maximum attempts. {nameof(MaxLoadSessionCookiesTrips)}={MaxLoadSessionCookiesTrips}");
            }
        }

        public async Task<LoginResult> SubmitCredentialsAsync(string email, string password)
        {
            // pre 1st visit: load init session cookies
            await LoadSessionCookiesAsync();

            // 1st visit: response = get 1st login page
            var login1_body = await getInitialLoginPage();

            // post 1st visit: set OAUTH_URL header
            // this will be our referer for all login calls AFTER initial oauth get
            loginClient.DefaultRequestHeaders.Add("Referer", _locale.OAuthUrl());

            var page = new CredentialsPage(loginClient, _systemDateTime, _locale, login1_body);
            return await page.SubmitAsync(email, password);
        }

        private async Task<string> getInitialLoginPage()
        {
            var response = await loginClient.GetAsync(_locale.OAuthUrl());
            response.EnsureSuccessStatusCode();

            var login1_body = await response.Content.ReadAsStringAsync();
            return login1_body;
        }
    }
}
