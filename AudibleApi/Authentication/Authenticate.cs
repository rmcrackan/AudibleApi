﻿using System;
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
        public IHttpClient LoginClient { get; }
        public ISystemDateTime SystemDateTime { get; }
        public Locale Locale { get; }

        public CookieCollection GetCookies(Uri uri) => LoginClient.CookieJar.GetCookies(uri);

        public Authenticate(Locale locale) : this(locale, ApiHttpClient.Create(), new SystemDateTime())
            => StackBlocker.ApiTestBlocker();

		public Authenticate(Locale locale, IHttpClient client, ISystemDateTime systemDateTime)
		{
			LoginClient = client ?? throw new ArgumentNullException(nameof(client));
			if (LoginClient.CookieJar.ReflectOverAllCookies().Count > 0)
				throw new ArgumentException("Cannot use a client which already has cookies");

            SystemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
            Locale = locale ?? throw new ArgumentNullException(nameof(locale));

            initClientState();
        }
		private void initClientState()
		{
            var baseUri = Locale.AmazonLoginUri();

            LoginClient.Timeout = new TimeSpan(0, 0, 30);
			LoginClient.BaseAddress = baseUri;

            LoginClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            LoginClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            LoginClient.DefaultRequestHeaders.Add("Accept-Language", Locale.Language);
            LoginClient.DefaultRequestHeaders.Add("Host", baseUri.Host);
            LoginClient.DefaultRequestHeaders.Add("Origin", baseUri.GetOrigin());
            LoginClient.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);
        }

        /// <summary>PUBLIC ENTRY POINT</summary>
        public static async Task<LoginResult> SubmitCredentialsAsync(Locale locale, string email, string password)
        {
            StackBlocker.ApiTestBlocker();

            var auth = new Authenticate(locale);
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
            while (!LoginClient.CookieJar.EnumerateCookies(Locale.AmazonLoginUri()).Any(c => c.Name.ToLower() == "session-token"))
            {
                await LoginClient.GetAsync(Locale.AmazonLoginUri());

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
            LoginClient.DefaultRequestHeaders.Add("Referer", Locale.OAuthUrl());

            var page = new CredentialsPage(this, login1_body);
            return await page.SubmitAsync(email, password);
        }

        private async Task<string> getInitialLoginPage()
        {
            var response = await LoginClient.GetAsync(Locale.OAuthUrl());
            response.EnsureSuccessStatusCode();

            var login1_body = await response.Content.ReadAsStringAsync();
            return login1_body;
        }
    }
}
