using System;
using AudibleApi.Authorization;
using Newtonsoft.Json.Linq;

namespace AuthorizationShared
{
    public static class Shared
    {
        public static string AccessTokenValue { get; } = @"
Atna|EwICIOT_xGxY2avbMSMTesbAtBhFwcFT83TbVpNIpLbZX6d41xePIBsKh_dT_H3lZ0kOOdVzrASg1zsxnrKqc8yhygMmHXCm5GhHpS0P79Hj0dNGqPm8WSoivu0RkViDdxF8EfoA2YoOFvDKPP-QxJCVlX6SD8M6u4UwcM57R5E1b59ZhB4jN3eAPdjSl6P7u2ccvFuL8y6yxZWIVkIWi7KHyJ2fO0GjmFKOmzhpsN6wzN6cI2lDfpFRTAoGfR452SG2am3wWpivqkXg_zyTY8qwm35R_JvxrjrG68DwVgI8tpf3fH625mBcuCcJRl8P9vCBgj5xAjodlctU3WlxXZaTENXGT-WZwDmi20mD3w9BdQgo-yeGqhNHK91Sj-SDU-kN0ICY1C1g1HXHyRt-wN9ttMAh8dzRmDrseh-AeVqU7QuPqQ".Trim();

        public static string RefreshTokenValue { get; } = @"
Atnr|EwICIHgTzgbB8DOLzNvmI_oIkP-jpN6_mLW_UlNxiHIUOujgt7tXB4meoch_ZfkaBFp9zYF1_pp0yEFWvEYuwtvoWrEpNsuR89C7Ki_lX9cClzodBfya6YaZPTT9R-X7cFBOlEVScFlEuDK0DRQ46NLsW935wzuYOKrXz_1LMt75OFd7Y3AlEdE18qv9mZxszvO9lDfOt0Xgkz3t5NGiQ1szCUl4TIyiKxpOn3fPkTiGMVzVYvJKhCG-zdaKQNVRKyWiN_U1EGoUsDMAiQSiL8aJaAEPSW8eVAqiSJAyOuGuoT4gPYLDju_wVNFbSVYbBK_fUxpwZXaHezt7Jdi_3RH3EDVJ8k06dTQmIIhcilRk7XvGsz-rm9JtTpxbCXMwhTONHaIeYogZHRiLWUEXX_mXVPSL_8U7UAr4BngNA0KdS84Waw
".Trim();

        public static string AdpTokenValue { get; } = @"
{enc:taDPLz1OS/IQwU8NDQIIOCv286ZnQ35y8jHJPb2FLud7RYuVOAJE3XkrkB/z4Z1jlUguegbtrQXuSdfwphi/Ld3FVZOS7HrAmD5iOAwkAHKfLsrD4c+T/3gJIibJQvO+1XjD7BHfUXqotzsJWA5VOEeLn980Fqj2TMqaYPoj+YhdLIgRUynpbd0mKlv3QBD/DvmRmwOYkKi6ewAFr656UeXPzNIntFuc7+WbP+QX28KvJJo0KxSNKQcX/FP2+u9vMFLPYEX4kDuYThMDtsU+rt3YwHKFhkQnuMU7wAh1xfWLtUCxXGiCjmEiupZ9rojnVId5W6AkooXMF7v9XznxR+HdCLlBlTXjKDu9u69QdhxFUoqbZafhqw7EBWKq2O3QrX4/6uI1FORUT1yFqow52N9nH3hM7w8+X6XxqvDQmU1l2mFk1NITYfINLEgb2vtVTpx7wz87pY8a8HbOQmxFdXKdFInRUv52mUVJkg6HWtXVU/2XWTiNLiT9JbZRSPZqJyTpezbzcQGyws78Ac2SMzSA9zpvbc8PgKOIa31m3TMwJ92z+HeppaKkZolSlGJNGtTtmzlBgptw4hlo2h2EFNM9HyGIxLfJqw6bNmR4iAu5gz+UaWUKnBRWEv1KAYRGnGWLN1LTG0X45O075YihJ1ybiVu8nmZqkGuP9Ve6yWM2bPPGKK8jvQVJzGK4jewyMj2iNq+xkiemhjvbAPMnsemNZWEBcLbX6hP/RhCmxhuU4DOKgpriWV2mXDbwWcr+RlGxOhEaOVyd26XWQA07LfdrSdw7qbcGEV2EL+PwgWKCOBIaxt95tzUZ4mJDXGDxDj2P6VGAelAeTZRetlgzuLpKRCGYnrBxyzXEhyOFEE25Lrgr3+1Dr+ESLOpoRYRz00k7lI/f0qvIFuiybb+U1rBPvV8MH0N/ysFjwLIXy12o2OwfGfz+57aS0cczfXx8e+Bd3mkAucp7hxOlCOOGWnaF/fyh3RD4AF0dTt8fqv0FMH7XJUagAnfjWA6GvFvKUH/CVQ/ICeXlsE0q7r90fg==}{key:UWCbi1gixyjn2h1O6Yru+3x0nyGdOQSVN4iY8SCS3TZ2IOs449FY9Tcro4KYlJTzlcr4JRzdcd7b7/LCVJVZUOhxZnAuLAdTOLruCjyFzKfOohViADUDKI2eZR5xSIerDPgS6aSECuwkmvjff53asC0TGEK4LafZ8jau+yN4SxQi5UmmXrODmfrt34sxYA8a6KXbVfPBZ3wzMEo5+IPnNa20Y662dh9p9HwHUqhjG5+VDotRgURU1b/TNwEsHssqkjvlGIG5M9v+Lz/yOEkI9JeKIhDmVQARscDsHKOCGx/loUCpqDsC3Hk/1Yv/Ln4yV2+SgqX5o5nbWZpMU5DQ/Q==}{iv:siWSUD2nXX7CKPm53NbYPQ==}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}
".Trim();

        public static string PrivateKeyValue { get; } = @"
-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEA/dX1T3gVl/GulHsWsBgLh8fSLQKq4ZPpUPuVkswArN4eDsPc\nSNRj4RpBRuEEIgA8rgB31Y2MYac4Z6nTlhEeKb7Ppq4fG5YAusFX0TfTbLYJ4Zz+\nqgRh6ZdDpov8f9YfY/Pfa5h7Oxmd2Rb6mv2OQeIkMlYeoQYaFhwQTqrqmZVRgfSQ\nJWx5Sycq+Ec5PI1sW2V5q82gcINBGPXAcBOaB9I/vTZ7UMfeGGLqx8O5GXBkzhpA\nl8OOARnzoYW6jHPm3EY08iEy/RZC3G8UDz0TJbKK4YxwDVHYTf18T2Ywi0UjJq8h\nbfOwneaMzPEf/K9xT7QAH7X1nELduHeuixRCewIDAQABAoIBADsQ2H5fgRbURD2E\nzui3D6fO1ZdnsX/APWB8yndYRSf0n6Xr6YyA76TnyRzHK4EF8RjEPx5QS8RFdxiY\nKgYXgZ9RKVyt08tFgnHyn46toOMBEReQwqmpT2ddrX5JwL22g6NskialWreL5HEp\nqbL7IWkvSCD2bTYnB1bxvNGxb9nAsI3geZex9ukRF83UnBjtwKKzDt+uZ6+m/OrS\nKNrRmP6GDf+5ow/SF/Tyzk5y8mPP5It4LbtGBkTPZ7xSfdj7WTzd3N0mpiJKRL+O\nsFeYe/u+P9p5LRnqLtpC4XDqsdoXriebc/hOq57pGGIjY8Ffvoehu7YRkwQ+IWj6\njI+xGtECgYEA/3LEfeebp46AfwHkq+os/zYyPDJhAdEO6QLOw9mYoq0WaqAcY0Vv\nPe9LL4Hk0GHexuor2V/XvndyLQc1M9fWRTVUfjQaEyPqzA/xYlwc53W9yxzDRlpT\nijIufvVBuBGmoZphSjyWDmNMbwBkSGX+KlYTqT2QxyYq1v9hkfxPCZcCgYEA/mJM\nlZRGjWWgtuzxnHAO8zs6zShbEjdl5daUPbzywrVutGqhP6ukPMxZYXnBNzu+dx+U\nZ8PLJq5j+y8TfHlwTZ0FfkmrWIFM62/e0yJoqdqOqHTUUQeCtQnRRxVhj9ze/0Kd\nwNYzxLdZog8Y4U/taNPzv2bf1VwOD4jGP01xAr0CgYB7JW7Ia6FEU5RqphUBM7Fh\nj1UEZB0T7R7NAgd/ryTAN8U4vdsxmEWajAOo8WvHcYceScG911CAh8DJKFJjnce0\nMZN8C84OMCB/I6hwjIt6oe5PPpx9DAp98tcraTy3afr0qSIB0dddNE2irYOqy1CO\nRFTSH9Xty96XKyJ0aDgKHQKBgQDWXYnCBwiJQRn07KkSNlAy4jfECPzt1ec1juSv\nPhmowPHAcZbeq8qkPWQYw1xyKhNwGRmbc6AvQYgZdOtL2p4rmWW2rWgKFjP2tvgk\nHfvrHrVW+dStT9Hys4o7B6aGcA8vNjjv9tH7NLMA4Q4LWKpsyye9pHh9OKzUpGLD\nl4PjVQKBgQCLbhhBPQUZookWK/7sBP8QUGlVHOBOPQLR52Iwp24Ew7S4/ejqdUMz\nAB2X5tKd5ShymWNspqJzTHlx8BUxA22Ml+jXsZQs1GGt7fVU9cjKzwmztBW63V0D\nCjZlLWPyXCbbdd8Yxz+tJ94gril7hHUaGCwalZ1aVR7+aJSPEI8VDA==\n-----END RSA PRIVATE KEY-----\n
".Trim();
        public static string PrivateKeyValueNewLines { get; } = PrivateKeyValue.Replace("\\n", "\n");

        public static string AccessTokenExpires { get; } = "2100-06-14T22:05:33.6204337-04:00";
        public static DateTime AccessTokenExpiresParsed { get; } = DateTime.Parse(AccessTokenExpires);

        public static string IdentityJson { get; }
            = new JObject {
                {
                    "ExistingAccessToken", new JObject {
                        { "TokenValue", AccessTokenValue },
                        { "Expires", AccessTokenExpires }
                    }
                },
                {
                    "PrivateKey", new JObject {
                        { "Value", PrivateKeyValueNewLines }
                    }
                },
                {
                    "AdpToken", new JObject {
                        { "Value", AdpTokenValue }
                    }
                },
                {
                    "RefreshToken", new JObject{
                        { "Value", RefreshTokenValue }
                    }
                },
                {
                    "Cookies", new JArray {
                        new JObject {
                            { "Key", "key1" },
                            { "Value", "value 1" }
                        },
                        new JObject {
                            { "Key", "key1" },
                            { "Value", "value 2" }
                        }
                    }
                }
            }.ToString().Replace("\\n", "\n");

        public static Identity GetIdentity()
			=> Identity.FromJson(IdentityJson);

        public static string AuthenticateResponse { get; } = @"
{
  ""response"": {
    ""success"": {
      ""extensions"": {
        ""device_info"": {
          ""device_name"": ""Robert's 3rd Audible for iPhone"",
          ""device_serial_number"": ""AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"",
          ""device_type"": ""A2CZJZGLK2JJVM""
        },
        ""customer_info"": {
          ""account_pool"": ""Amazon"",
          ""user_id"": ""amzn1.account.AGOMUXRDACRMEC4RLNMOTVVGSMUQ"",
          ""home_region"": ""NA"",
          ""name"": ""Robert McRackan"",
          ""given_name"": ""Robert""
        }
      },
      ""tokens"": {
        ""website_cookies"": [
          {
            ""Path"": ""/"",
            ""Secure"": ""false"",
            ""Value"": ""138-0043508-7748562"",
            ""Expires"": ""4 Jul 2039 16:22:25 GMT"",
            ""Domain"": "".amazon.com"",
            ""HttpOnly"": ""false"",
            ""Name"": ""session-id""
          },
          {
            ""Path"": ""/"",
            ""Secure"": ""false"",
            ""Value"": ""132-5048099-5575317"",
            ""Expires"": ""4 Jul 2039 16:22:25 GMT"",
            ""Domain"": "".amazon.com"",
            ""HttpOnly"": ""false"",
            ""Name"": ""ubid-main""
          },
          {
            ""Path"": ""/"",
            ""Secure"": ""false"",
            ""Value"": ""\""T?e@Dhoe79JtrG79gBmXvWBhBLmFOBMd\"""",
            ""Expires"": ""4 Jul 2039 16:22:25 GMT"",
            ""Domain"": "".amazon.com"",
            ""HttpOnly"": ""false"",
            ""Name"": ""x-main""
          },
          {
            ""Path"": ""/"",
            ""Secure"": ""true"",
            ""Value"": ""\""Atza|IwEBIBTZIb_oWupMlu4n10seFxU4BMj9MJIMM3ks7HJYjzAn-26eNufQ213Jm9ZbIU6AFB8YOlTDUA-qelUNEsLLo657O8AwCQXKB7u7yfo2iA2P8s3IajyJ1iVDqgEKzn6poaEKoZqxen7hN5CHXWHMnAn7fQMn2x1MOdj-v3jabLKUw7iLqwsizfJFDjzZfXuG423Lkm_gv--9pzVNMPYzN-FDX1ctac4ECmRQ_U5KUnraWngdfqvx8MU3PO4oH6zNEvr_U8vo2qsq6wXo0d_-mVHcWehTM4pF6HdYn18JrI7l46eueHmPojg3x-O7QAOanavstBL7OLoyZszdv6Z02LqSHSMaGq8xY3FD_I7GTl5x8ukWjyXrQyLyypMCa61Dj5iauA1V0DbxL2F8FqtFJGjNZPm4koKiG9daTDqKZE4AXGGDj3lwMffG3bVQfOYnU1jAV5-rJZ-IYbh76WjMqPzRzqDGHmSGBwfzJJIWKknMes9LCkJVPqHoKhNB3y7W0Ow\"""",
            ""Expires"": ""9 Jul 2019 17:22:25 GMT"",
            ""Domain"": "".amazon.com"",
            ""HttpOnly"": ""true"",
            ""Name"": ""at-main""
          },
          {
            ""Path"": ""/"",
            ""Secure"": ""true"",
            ""Value"": ""\""V2H+r5DflutVy7YIRgr2LW5eeMjy9OdEMeG8JVj66Ok=\"""",
            ""Expires"": ""9 Jul 2019 17:22:25 GMT"",
            ""Domain"": "".amazon.com"",
            ""HttpOnly"": ""true"",
            ""Name"": ""sess-at-main""
          }
        ],
        ""mac_dms"": {
          ""device_private_key"": """ + PrivateKeyValue + @""",
          ""adp_token"": """ + AdpTokenValue + @"""
        },
        ""bearer"": {
          ""access_token"": """ + AccessTokenValue + @""",
          ""refresh_token"": """ + RefreshTokenValue + @""",
          ""expires_in"": ""3600""
        }
      },
      ""customer_id"": ""amzn1.account.AGOMUXRDACRMEC4RLNMOTVVGSMUQ""
    }
  },
  ""request_id"": ""b47a1a95-f9b6-4715-a609-4304dd050d7e""
}
".Trim();

        public static string RefreshTokenResponse { get; }
            = new JObject
            {
                { "access_token", AccessTokenValue },
                { "token_type", "bearer" },
                { "expires_in", 3600 }
            }
            .ToString();
    }
}
