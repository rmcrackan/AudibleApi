using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authentication
{
    public class Authenticate
    {
        public IHttpClient LoginClient { get; }
        public ISystemDateTime SystemDateTime { get; }
        public Locale Locale { get; }

		public string DeviceSerialNumber { get; }
		public string CodeVerifier { get; }

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

            DeviceSerialNumber = build_device_serial();
            CodeVerifier = create_code_verifier();

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
            LoginClient.CookieJar.Add(buildInitCookies());
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
            var response = await LoginClient.GetAsync(Locale.OAuthUrl(DeviceSerialNumber, CodeVerifier));
            response.EnsureSuccessStatusCode();

            var login1_body = await response.Content.ReadAsStringAsync();
            return login1_body;
        }

		private CookieCollection buildInitCookies()
        {
            //Build initial cookies to prevent captcha in most cases
            //https://github.com/mkb79/Audible/blob/master/src/audible/login.py

            var frc = new byte[313];
            new Random().NextBytes(frc);

            var mapMd = new JObject
            {
                { "device_user_dictionary", new JArray() },
                { "device_registration_data", 
                    new JObject {
                        {"software_version", Resources.SoftwareVersion }
                    }
                },
                { "app_identifier",
                    new JObject {
                        {"app_version", Resources.AppVersion },
                        {"bundle_id", "com.audible.iphone" }
                    }
                }
            };

            var frcStr = Convert.ToBase64String(frc).TrimEnd('=');
            var mapMdStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(mapMd.ToString(Newtonsoft.Json.Formatting.None))).TrimEnd('=');
            var amznAppId = "MAPiOSLib/6.0/ToHideRetailLink";
            var cookieDomain = $".{Locale.LoginDomain()}.{Locale.TopDomain}";

            var initCookies = new CookieCollection
            {
                new Cookie("frc", frcStr, "/", cookieDomain),
                new Cookie("map-md", mapMdStr,"/", cookieDomain),
                new Cookie("amzn-app-id", amznAppId,"/", cookieDomain),
            };

            return initCookies;
        }


		//https://github.com/mkb79/Audible/blob/master/src/audible/login.py
		public static string build_device_serial() => Guid.NewGuid().ToString("N").ToUpper();

		public static string create_code_verifier()
		{
			var code_verifier = new byte[32];
			new Random().NextBytes(code_verifier);
            return Convert.ToBase64String(code_verifier).Replace('+', '-').Replace('/', '_').TrimEnd('=');
		}
	}
}
