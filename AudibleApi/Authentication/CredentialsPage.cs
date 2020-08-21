using System;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authentication
{
	public class CredentialsPage : LoginResult
	{
		public CredentialsPage(IHttpClient client, ISystemDateTime systemDateTime, string responseBody) : base(client, systemDateTime, responseBody) { }

		public async Task<LoginResult> SubmitAsync(string email, string password)
		{
			if (email is null)
				throw new ArgumentNullException(nameof(email));
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Password may not be blank", nameof(email));

			if (password is null)
				throw new ArgumentNullException(nameof(password));
			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password may not be blank", nameof(password));

			Inputs["email"] = email;
			Inputs["password"] = password;
			Inputs["metadata1"] = getEncryptedMetadata(SystemDateTime.UtcNow.ToUnixTimeStamp());

			return await GetResultsPageAsync(Inputs);
		}

		private static string getEncryptedMetadata(long nowUnixTimeStamp)
		{
			var raw_metadata = GenerateMetadata(nowUnixTimeStamp);
			var metadata = Cryptography.EncryptMetadata(raw_metadata);
			return metadata;
		}

		public static string GenerateMetadata(long nowUnixTimeStamp)
		{
			var raw = new JObject {
				{ "start", nowUnixTimeStamp },
				{ "interaction",
					new JObject {
						{ "keys", 0 },
						{ "keyPressTimeIntervals", new JArray() },
						{ "copies", 0 },
						{ "cuts", 0 },
						{ "pastes", 0 },
						{ "clicks", 0 },
						{ "touches", 0 },
						{ "mouseClickPositions", new JArray() },
						{ "keyCycles", new JArray() },
						{ "mouseCycles", new JArray() },
						{ "touchCycles", new JArray() },
					}
				},
				{ "version", "3.0.0" },
				{ "lsUbid", "X39-6721012-8795219:1549849158" },
				{ "timeZone", -6 },
				{ "scripts",
					new JObject {
						{ "dynamicUrls",
							new JArray {
								"https://images-na.ssl-images-amazon.com/images/I/61HHaoAEflL._RC|11-BZEJ8lnL.js,01qkmZhGmAL.js,71qOHv6nKaL.js_.js?AUIClients/AudibleiOSMobileWhiteAuthSkin#mobile",
								"https://images-na.ssl-images-amazon.com/images/I/21T7I7qVEeL._RC|21T1XtqIBZL.js,21WEJWRAQlL.js,31DwnWh8lFL.js,21VKEfzET-L.js,01fHQhWQYWL.js,51TfwrUQAQL.js_.js?AUIClients/AuthenticationPortalAssets#mobile",
								"https://images-na.ssl-images-amazon.com/images/I/0173Lf6yxEL.js?AUIClients/AuthenticationPortalInlineAssets",
								"https://images-na.ssl-images-amazon.com/images/I/211S6hvLW6L.js?AUIClients/CVFAssets",
								"https://images-na.ssl-images-amazon.com/images/G/01/x-locale/common/login/fwcim._CB454428048_.js"
							}
						},
						{ "inlineHashes",
							new JArray {
								-1746719145,
								1334687281,
								-314038750,
								1184642547,
								-137736901,
								318224283,
								585973559,
								1103694443,
								11288800,
								-1611905557,
								1800521327,
								-1171760960,
								-898892073
							}
						},
						{ "elapsed", 52 },
						{ "dynamicUrlCount", 5 },
						{ "inlineHashesCount", 13 }
					}
				},
				{ "plugins", "unknown||320-568-548-32-*-*-*" },
				{ "dupedPlugins", "unknown||320-568-548-32-*-*-*" },
				{ "screenInfo", "320-568-548-32-*-*-*" },
				{ "capabilities",
					new JObject {
						{ "js",
							new JObject {
								{ "audio", true },
								{ "geolocation", true },
								{ "localStorage", "supported" },
								{ "touch", true },
								{ "video", true },
								{ "webWorker", true }
							}
						},
						{ "css",
							new JObject {
								{ "textShadow", true },
								{ "textStroke", true },
								{ "boxShadow", true },
								{ "borderRadius", true },
								{ "borderImage", true },
								{ "opacity", true },
								{ "transform", true },
								{ "transition", true }
							}
						},
						{ "elapsed", 1 }
					}
				},
				{ "referrer", "" },
				{ "userAgent", Resources.UserAgent },
				{ "location", Resources.STATIC_OAuthUrl },
				{ "webDriver", null },
				{  "history",
					new JObject {
						{ "length", 1 }
					}
				},
				{ "gpu",
					new JObject {
						{ "vendor", "Apple Inc." },
						{ "model", "Apple A9 GPU" },
						{ "extensions", new JArray() }
					}
				},
				{ "math",
					new JObject {
						{ "tan", "-1.4214488238747243" },
						{ "sin", "0.8178819121159085" },
						{ "cos", "-0.5753861119575491" }
					}
				},
				{ "performance",
					new JObject {
						{ "timing",
							new JObject {
								{ "navigationStart", nowUnixTimeStamp },
								{ "unloadEventStart", 0 },
								{ "unloadEventEnd", 0 },
								{ "redirectStart", 0 },
								{ "redirectEnd", 0 },
								{ "fetchStart", nowUnixTimeStamp },
								{ "domainLookupStart", nowUnixTimeStamp },
								{ "domainLookupEnd", nowUnixTimeStamp },
								{ "connectStart", nowUnixTimeStamp },
								{ "connectEnd", nowUnixTimeStamp },
								{ "secureConnectionStart", nowUnixTimeStamp },
								{ "requestStart", nowUnixTimeStamp },
								{ "responseStart", nowUnixTimeStamp },
								{ "responseEnd", nowUnixTimeStamp },
								{ "domLoading", nowUnixTimeStamp },
								{ "domInteractive", nowUnixTimeStamp },
								{ "domContentLoadedEventStart", nowUnixTimeStamp },
								{ "domContentLoadedEventEnd", nowUnixTimeStamp },
								{ "domComplete", nowUnixTimeStamp },
								{ "loadEventStart", nowUnixTimeStamp },
								{ "loadEventEnd", nowUnixTimeStamp }
							}
						}
					}
				},
				{ "end", nowUnixTimeStamp },
				{ "timeToSubmit", 108873 },
				{ "form",
					new JObject {
						{ "email",
							new JObject {
								{ "keys", 0 },
								{ "keyPressTimeIntervals", new JArray() },
								{ "copies", 0 },
								{ "cuts", 0 },
								{ "pastes", 0 },
								{ "clicks", 0 },
								{ "touches", 0 },
								{ "mouseClickPositions", new JArray() },
								{ "keyCycles", new JArray() },
								{ "mouseCycles", new JArray() },
								{ "touchCycles", new JArray() },
								{ "width", 290 },
								{ "height", 43 },
								{ "checksum", "C860E86B" },
								{ "time", 12773 },
								{ "autocomplete", false },
								{ "prefilled", false }
							}
						},
						{ "password",
							new JObject {
								{ "keys", 0 },
								{ "keyPressTimeIntervals", new JArray() },
								{ "copies", 0 },
								{ "cuts", 0 },
								{ "pastes", 0 },
								{ "clicks", 0 },
								{ "touches", 0 },
								{ "mouseClickPositions", new JArray() },
								{ "keyCycles", new JArray() },
								{ "mouseCycles", new JArray() },
								{ "touchCycles", new JArray() },
								{ "width", 290 },
								{ "height", 43 },
								{ "time", 10353 },
								{ "autocomplete", false },
								{ "prefilled", false }
							}
						},
					}
				},
				{ "canvas",
					new JObject {
						{ "hash", -373378155 },
						{ "emailHash", -1447130560 },
						{ "histogramBins", new JArray() }
					}
				},
				{ "token", null },
				{ "errors", new JArray() },
				{ "metrics",
					new JArray {
						new JObject {
							{ "n", "fwcim-mercury-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-instant-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-element-telemetry-collector" },
							{ "t", 2 }
						},
						new JObject {
							{ "n", "fwcim-script-version-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-local-storage-identifier-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-timezone-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-script-collector" },
							{ "t", 1 }
						},
						new JObject {
							{ "n", "fwcim-plugin-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-capability-collector" },
							{ "t", 1 }
						},
						new JObject {
							{ "n", "fwcim-browser-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-history-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-gpu-collector" },
							{ "t", 1 }
						},
						new JObject {
							{ "n", "fwcim-battery-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-dnt-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-math-fingerprint-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-performance-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-timer-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-time-to-submit-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-form-input-telemetry-collector" },
							{ "t", 4 }
						},
						new JObject {
							{ "n", "fwcim-canvas-collector" },
							{ "t", 2 }
						},
						new JObject {
							{ "n", "fwcim-captcha-telemetry-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-proof-of-work-collector" },
							{ "t", 1 }
						},
						new JObject {
							{ "n", "fwcim-ubf-collector" },
							{ "t", 0 }
						},
						new JObject {
							{ "n", "fwcim-timer-collector" },
							{ "t", 0 }
						}
					}
				}
			};

			// TestCommon > JsonExamples > ToString > metadata()
			// verifies that this syntax produces a string identical to the hand-crafted version
			var debug = raw.ToString(Formatting.Indented);

			var str = raw.ToString(Formatting.None);
			return str;
		}
	}
}
