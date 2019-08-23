using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace JsonExamples
{
    [TestClass]
    public class attributes
    {
        public class JsonTest
        {
            [JsonProperty]
            private string nullPriv;

            [JsonProperty]
            private int priv;
            public void setPriv(int p) => priv = p;
            public int GetPriv() => priv;

            // will serialize. cannot deserialize
            public double GetPrivSet { get; private set; }
            public void setGPS(double gps) => GetPrivSet = gps;

            [JsonRequired]
            private string privReq = "q";
            public void SetPrivReq(string pr) => privReq = pr;
            public string GetPrivReq() => privReq;

            [JsonProperty]
            private ComplexType complexType;
            public void SetComplexType(ComplexType ct) => complexType = ct;
            public ComplexType GetComplexType() => complexType;

            [JsonIgnore]
            public string IgnoreMe { get; set; }

            [JsonConstructor]
            private JsonTest(int getOnly) => GetOnly = getOnly;
            // normal serialize. deser via ctor
            public int GetOnly { get; }

            public static JsonTest Factory(int i) => new JsonTest(i);
        }
        public class ComplexType
        {
            public string Value { get; set; }
            public DateTime Expires { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class OptIn
        {
            // excluded from serialization
            // does not have JsonPropertyAttribute
            public Guid Id { get; set; }

            [JsonProperty]
            public string Name { get; set; }

            [JsonProperty]
            public int Size { get; set; }
        }

        [TestMethod]
        public void decorated_private_ctor()
        {
            var j = JsonTest.Factory(9);
            var ser = JsonConvert.SerializeObject(j);
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);

            deser.GetOnly.Should().Be(9);
        }

        [TestMethod]
        public void nullPriv_is_serialized()
        {
            var j = JsonTest.Factory(9);
            var ser = JsonConvert.SerializeObject(j);

            ser.Should().Contain("\"nullPriv\":null");
        }

        [TestMethod]
        public void priv_is_deserialized()
        {
            var j = JsonTest.Factory(9);
            j.setPriv(2);
            var ser = JsonConvert.SerializeObject(j);
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);
            deser.GetPriv().Should().Be(2);
        }

        [TestMethod]
        public void GetPrivSet_cannot_be_deserialized()
        {
            var j = JsonTest.Factory(9);
            j.setGPS(5);
            j.GetPrivSet.Should().Be(5);
            j.GetPrivSet.Should().Be(5d);

            // is serialized
            var ser = JsonConvert.SerializeObject(j);
            ser.Should().Contain("\"" + nameof(j.GetPrivSet) + "\":5.0");

            // cannot be deserialized
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);
            deser.GetPrivSet.Should().Be(default);
        }

        [TestMethod]
        public void priv_JsonRequired_is_ser_and_deser()
        {
            var j = JsonTest.Factory(9);
            j.SetPrivReq("y");
            j.GetPrivReq().Should().Be("y");

            var ser = JsonConvert.SerializeObject(j);
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);
            deser.GetPrivReq().Should().Be("y");
        }

        [TestMethod]
        public void priv_JsonRequired_null_throws_on_serialize()
        {
            var j = JsonTest.Factory(9);
            j.SetPrivReq(null);
            Assert.ThrowsException<JsonSerializationException>(() => JsonConvert.SerializeObject(j));
        }

        [TestMethod]
        public void complex_type_is_ser_and_deser()
        {
            var j = JsonTest.Factory(9);
            j.GetComplexType().Should().BeNull();

            var ser = JsonConvert.SerializeObject(j);
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);
            deser.GetComplexType().Should().BeNull();

            var token = new ComplexType { Value = "v", Expires = DateTime.MaxValue };
            deser.SetComplexType(token);
            var ser2 = JsonConvert.SerializeObject(deser);
            var deser2 = JsonConvert.DeserializeObject<JsonTest>(ser2);
            deser2.GetComplexType().Value.Should().Be("v");
            deser2.GetComplexType().Expires.Should().Be(DateTime.MaxValue);
        }

        [TestMethod]
        public void ignore_property()
        {
            var j = JsonTest.Factory(9);
            j.IgnoreMe = "hi!";
            j.IgnoreMe.Should().Be("hi!");

            var ser = JsonConvert.SerializeObject(j);
            ser.Should().NotContain(nameof(j.IgnoreMe));
            var deser = JsonConvert.DeserializeObject<JsonTest>(ser);
            deser.IgnoreMe.Should().BeNull();
        }

        [TestMethod]
        public void opt_in()
        {
            var o = new OptIn { Id = Guid.NewGuid(), Name = "n", Size = 7 };
            var ser = JsonConvert.SerializeObject(o);
            ser.Should().NotContain(nameof(o.Id));
            var deser = JsonConvert.DeserializeObject<OptIn>(ser);
            deser.Name.Should().Be("n");
            deser.Size.Should().Be(7);
            Assert.AreEqual(deser.Id, default);
        }
    }

	[TestClass]
	public class sealed_without_attributes
	{
		public class _1Param
		{
			public string Bar { get; }
			public _1Param(string bar) => Bar = bar;
		}

		[TestMethod]
		public void ctor_1_param()
			=> JsonConvert
				.DeserializeObject<_1Param>("{ \"bar\": \"hello\" }")
				.Bar.Should().Be("hello");

		public class _2Params
		{
			public string Foo { get; }
			public string Bar { get; }
			public _2Params(string foo, string bar)
			{
				Foo = foo;
				Bar = bar;
			}
		}

		[TestMethod]
		public void match_param_order_match_case()
		{
			var obj = JsonConvert.DeserializeObject<_2Params>("{ \"foo\":\"f\" , \"bar\":\"b\" }");
			obj.Foo.Should().Be("f");
			obj.Bar.Should().Be("b");
		}

		[TestMethod]
		public void match_param_order_not_match_case()
		{
			var obj = JsonConvert.DeserializeObject<_2Params>("{ \"FOO\":\"f\" , \"bAr\":\"b\" }");
			obj.Foo.Should().Be("f");
			obj.Bar.Should().Be("b");
		}

		[TestMethod]
		public void not_match_param_order_match_case()
		{
			var obj = JsonConvert.DeserializeObject<_2Params>("{ \"bar\":\"b\" , \"foo\":\"f\" }");
			obj.Foo.Should().Be("f");
			obj.Bar.Should().Be("b");
		}

		[TestMethod]
		public void not_match_param_order_not_match_case()
		{
			var obj = JsonConvert.DeserializeObject<_2Params>("{ \"bAR\":\"b\" , \"Foo\":\"f\" }");
			obj.Foo.Should().Be("f");
			obj.Bar.Should().Be("b");
		}
	}

    [TestClass]
    public class create_syntax
    {
        [TestMethod]
        public void manually_create()
        {
            // https://www.newtonsoft.com/json/help/html/CreateJsonManually.htm

            var o = new JObject();
            o.Add("a", 1);

            JArray array = new JArray();
            array.Add("Manual text");
            array.Add(new DateTime(2000, 5, 23));
            o["MyArray"] = array;

            var str = o.ToString(Formatting.None);
            var expected = @"
{""a"":1,""MyArray"":[""Manual text"",""2000-05-23T00:00:00""]}".Trim();

            str.Should().Be(expected);
        }

        [TestMethod]
        public void collection_initializer()
        {
            // https://www.newtonsoft.com/json/help/html/CreateJsonCollectionInitializer.htm

            var o = new JObject
            {
                { "Cpu", "Intel" },
                { "Memory", 32 },
                {
                    "Drives", new JArray
                    {
                        "DVD",
                        "SSD"
                    }
                }
            };

            var str = o.ToString(Formatting.None);
            var expected = @"
{""Cpu"":""Intel"",""Memory"":32,""Drives"":[""DVD"",""SSD""]}".Trim();

            str.Should().Be(expected);
        }

        [TestMethod]
        public void dynamic()
        {
            // https://www.newtonsoft.com/json/help/html/CreateJsonDynamic.htm

            dynamic o = new JObject();
            o.ProductName = "Elbow Grease";
            o.Enabled = true;
            o.Price = 4.90m;
            o.StockCount = 9000;
            o.StockValue = 44100;
            o.Tags = new JArray("Real", "OnSale");

            string str = o.ToString(Formatting.None);
            var expected = @"
{""ProductName"":""Elbow Grease"",""Enabled"":true,""Price"":4.90,""StockCount"":9000,""StockValue"":44100,""Tags"":[""Real"",""OnSale""]}".Trim();

            str.Should().Be(expected);
        }

        [TestMethod]
        public void mixed()
        {
            // strong typed instance
            var jsonObject = new JObject();

            // explicitly add values using class interface
            jsonObject.Add("Entered", new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            // cast to dynamic to dynamically add/read properties
            dynamic album = jsonObject;

            album.AlbumName = "Closing Time";
            album.Artist = "Tom Waits";
            album.YearReleased = 1973;

            album.Songs = new JArray() as dynamic;

            dynamic song1 = new JObject();
            song1.SongName = "Ol' '55";
            song1.SongLength = "3:58";
            album.Songs.Add(song1);

            dynamic song2 = new JObject();
            song2.SongName = "I Hope That I Don't Fall in Love with You";
            song2.SongLength = "3:54";
            album.Songs.Add(song2);

            string output = album.ToString();

            var expected = @"
{
  ""Entered"": ""2000-01-01T12:00:00Z"",
  ""AlbumName"": ""Closing Time"",
  ""Artist"": ""Tom Waits"",
  ""YearReleased"": 1973,
  ""Songs"": [
    {
      ""SongName"": ""Ol' '55"",
      ""SongLength"": ""3:58""
    },
    {
      ""SongName"": ""I Hope That I Don't Fall in Love with You"",
      ""SongLength"": ""3:54""
    }
  ]
}
".Trim();
            output.Should().Be(expected);
        }
    }

    // diff b/t JToken, JContainer, JArray, JObject, JProperty, JValue
    // https://stackoverflow.com/a/38560188
    [TestClass]
    public class parsing
    {
        [TestMethod]
        public void known_JObject_strongly_typed()
        {
            // curly braces: we have an object
            var jObj = @"
                {
                    ""Id"": 9,
                    ""Name"":""Rick"",
                    ""Company"":""West Wind""
                }";

            var j = JObject.Parse(jObj);

            // direct cast
            var _9 = (int)j["Id"];
            _9.Should().Be(9);
            var rick = (string)j["Name"];
            rick.Should().Be("Rick");

            // or with ToString
            var westWind = j["Company"].ToString();
            westWind.Should().Be("West Wind");
        }

        [TestMethod]
        public void known_JObject_dynamic()
        {
            // curly braces: we have an object
            var json = @"
                {
                    ""Name"":""Rick"",
                    ""Company"":""West Wind"",
                    ""Entered"":""2012-03-16T00:03:33.245-10:00""
                }";

            dynamic j = JObject.Parse(json);

            // cast
            string name = j.Name;
            name.Should().Be("Rick");
            DateTime entered = j.Entered;
            entered.Should().Be(DateTime.Parse("2012-03-16T00:03:33.245-10:00"));
            string w = j.Company;
            w.Should().Be("West Wind");
        }

        [TestMethod]
        public void known_JArray_strongly_typed()
        {
            // square brackets: we have an array
            var json = @"
[
  {
    ""Name"": ""Bill"",
    ""Company"": ""Microsoft""
  },
  {
    ""Name"": ""Steve"",
    ""Company"": ""Apple""
  }
]";

            var techGuys = JArray.Parse(json);

            // direct cast
            var bill = (string)techGuys[0]["Name"];
            bill.Should().Be("Bill");
            var ms = (string)techGuys[0]["Company"];
            ms.Should().Be("Microsoft");

            // or ToString
            techGuys[1]["Name"].ToString().Should().Be("Steve");
            techGuys[1]["Company"].ToString().Should().Be("Apple");
        }

        [TestMethod]
        public void known_JArray_dynamic()
        {
            // square brackets: we have an array
            var json = @"
[
  {
    ""who"": ""grandpa"",
    ""age"": 99
  },
  {
    ""who"": ""future human"",
    ""age"": -1
  }
]";

            dynamic array = JArray.Parse(json);

            string grandpa = array[0].who;
            grandpa.Should().Be("grandpa");
            int gAge = array[0].age;
            gAge.Should().Be(99);

            string futureHuman = array[1].who;
            futureHuman.Should().Be("future human");
            int fhAge = array[1].age;
            fhAge.Should().Be(-1);
        }

        [TestMethod]
        public void unknown_type_object()
        {
            var obj = @"
                {
                    ""Name"": ""Rick"",
                    ""Company"": ""West Wind""
                }";

            var token = JToken.Parse(obj);
            token.Type.Should().Be(JTokenType.Object);

            var cast = (JObject)token;

            cast["Name"].ToString().Should().Be("Rick");
            cast["Company"].ToString().Should().Be("West Wind");
        }

        [TestMethod]
        public void unknown_type_array()
        {
            var array = @"
[
  {
    ""Name"": ""Bill"",
    ""Company"": ""Microsoft""
  },
  {
    ""Name"": ""Steve"",
    ""Company"": ""Apple""
  }
]";

            var token = JToken.Parse(array);
            token.Type.Should().Be(JTokenType.Array);

            var cast = (JArray)token;

            cast[0]["Name"].ToString().Should().Be("Bill");
            cast[0]["Company"].ToString().Should().Be("Microsoft");
            cast[1]["Name"].ToString().Should().Be("Steve");
            cast[1]["Company"].ToString().Should().Be("Apple");
        }

        // below examples adapted from:
        // https://weblog.west-wind.com/posts/2012/aug/30/using-jsonnet-for-dynamic-json-parsing
        public class Album
        {
            public string AlbumName { get; set; }
            public string Artist { get; set; }
            public int YearReleased { get; set; }
            public List<Song> Songs { get; set; } = new List<Song>();
        }

        public class Song
        {
            public string SongName { get; set; }
            public string SongLength { get; set; }
        }

        [TestMethod]
        public void parse_to_strong_type()
        {
            var json = @"
[
  {
    ""AlbumName"": ""Dirty Deeds Done Dirt Cheap"",
    ""Artist"": ""AC/DC"",
    ""YearReleased"": 1976,
    ""Songs"": [
      {
        ""SongName"": ""Dirty Deeds Done Dirt Cheap"",
        ""SongLength"": ""4:11""
      },
      {
        ""SongName"": ""Love at First Feel"",
        ""SongLength"": ""3:10""
      }
    ]
  },
  {
    ""AlbumName"": ""End of the Silence"",
    ""Artist"": ""Henry Rollins Band"",
    ""YearReleased"": 1992,
    ""Songs"": [
      {
        ""SongName"": ""Low Self Opinion"",
        ""SongLength"": ""5:24""
      },
      {
        ""SongName"": ""Grip"",
        ""SongLength"": ""4:51""
      }
    ]
  }
]";
            var albums = JArray.Parse(json);

            // pick out one album
            var jalbum = albums[0] as JObject;

            // Copy to a new Album instance
            var album = jalbum.ToObject<Album>();

            Assert.IsNotNull(album);
            Assert.AreEqual(album.AlbumName, jalbum.Value<string>("AlbumName"));
            Assert.IsTrue(album.Songs.Count > 0);
        }

        [TestMethod]
        public void strongly_typed_serialization()
        {
            // Demonstrate deserialization from a raw string
            var album = new Album()
            {
                AlbumName = "Dirty Deeds Done Dirt Cheap",
                Artist = "AC/DC",
                YearReleased = 1976,
                Songs = new List<Song>
                {
                    new Song
                    {
                        SongName = "Dirty Deeds Done Dirt Cheap",
                        SongLength = "4:11"
                    },
                    new Song
                    {
                        SongName = "Love at First Feel",
                        SongLength = "3:10"
                    }
                }
            };

            // serialize to string            
            var json2 = JsonConvert.SerializeObject(album, Formatting.Indented);

            Console.WriteLine(json2);

            // make sure we can serialize back
            var album2 = JsonConvert.DeserializeObject<Album>(json2);

            Assert.IsNotNull(album2);
            Assert.IsTrue(album2.AlbumName == "Dirty Deeds Done Dirt Cheap");
            Assert.IsTrue(album2.Songs.Count == 2);
        }
    }

	[TestClass]
	public class ToString
	{
		[TestMethod]
		public void metadata()
		{
			var csharpSyntax = getRawMetadata("userAgent", "oauthUrl", 123456789);
			var stringLiteral = getMetadata("userAgent", "oauthUrl", 123456789);

			csharpSyntax.Should().Be(stringLiteral);
		}

		private static string getRawMetadata(string useragent, string oauth, long nowUnixTimeStamp)
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
				{ "userAgent", useragent },
				{ "location", oauth },
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

			var debug = raw.ToString(Formatting.Indented);

			var str = raw.ToString(Formatting.None);
			return str;
		}
		private string getMetadata(string useragent, string oauth, long nowUnixTimeStamp)
		{
			var raw_metadata =
			#region
				$"{{\"start\":{nowUnixTimeStamp},\"interaction\":{{\"keys\":0,\"keyPressTimeIntervals\":[],\"copies\":0,\"cuts\":0,\"pastes\":0,\"clicks\":0,\"touches\":0,\"mouseClickPositions\":[],\"keyCycles\":[],\"mouseCycles\":[],\"touchCycles\":[]}},\"version\":\"3.0.0\",\"lsUbid\":\"X39-6721012-8795219:1549849158\",\"timeZone\":-6,\"scripts\":{{\"dynamicUrls\":[\"https://images-na.ssl-images-amazon.com/images/I/61HHaoAEflL._RC|11-BZEJ8lnL.js,01qkmZhGmAL.js,71qOHv6nKaL.js_.js?AUIClients/AudibleiOSMobileWhiteAuthSkin#mobile\",\"https://images-na.ssl-images-amazon.com/images/I/21T7I7qVEeL._RC|21T1XtqIBZL.js,21WEJWRAQlL.js,31DwnWh8lFL.js,21VKEfzET-L.js,01fHQhWQYWL.js,51TfwrUQAQL.js_.js?AUIClients/AuthenticationPortalAssets#mobile\",\"https://images-na.ssl-images-amazon.com/images/I/0173Lf6yxEL.js?AUIClients/AuthenticationPortalInlineAssets\",\"https://images-na.ssl-images-amazon.com/images/I/211S6hvLW6L.js?AUIClients/CVFAssets\",\"https://images-na.ssl-images-amazon.com/images/G/01/x-locale/common/login/fwcim._CB454428048_.js\"],\"inlineHashes\":[-1746719145,1334687281,-314038750,1184642547,-137736901,318224283,585973559,1103694443,11288800,-1611905557,1800521327,-1171760960,-898892073],\"elapsed\":52,\"dynamicUrlCount\":5,\"inlineHashesCount\":13}},\"plugins\":\"unknown||320-568-548-32-*-*-*\",\"dupedPlugins\":\"unknown||320-568-548-32-*-*-*\",\"screenInfo\":\"320-568-548-32-*-*-*\",\"capabilities\":{{\"js\":{{\"audio\":true,\"geolocation\":true,\"localStorage\":\"supported\",\"touch\":true,\"video\":true,\"webWorker\":true}},\"css\":{{\"textShadow\":true,\"textStroke\":true,\"boxShadow\":true,\"borderRadius\":true,\"borderImage\":true,\"opacity\":true,\"transform\":true,\"transition\":true}},\"elapsed\":1}},\"referrer\":\"\",\"userAgent\":\"{useragent}\",\"location\":\"{oauth}\",\"webDriver\":null,\"history\":{{\"length\":1}},\"gpu\":{{\"vendor\":\"Apple Inc.\",\"model\":\"Apple A9 GPU\",\"extensions\":[]}},\"math\":{{\"tan\":\"-1.4214488238747243\",\"sin\":\"0.8178819121159085\",\"cos\":\"-0.5753861119575491\"}},\"performance\":{{\"timing\":{{\"navigationStart\":{nowUnixTimeStamp},\"unloadEventStart\":0,\"unloadEventEnd\":0,\"redirectStart\":0,\"redirectEnd\":0,\"fetchStart\":{nowUnixTimeStamp},\"domainLookupStart\":{nowUnixTimeStamp},\"domainLookupEnd\":{nowUnixTimeStamp},\"connectStart\":{nowUnixTimeStamp},\"connectEnd\":{nowUnixTimeStamp},\"secureConnectionStart\":{nowUnixTimeStamp},\"requestStart\":{nowUnixTimeStamp},\"responseStart\":{nowUnixTimeStamp},\"responseEnd\":{nowUnixTimeStamp},\"domLoading\":{nowUnixTimeStamp},\"domInteractive\":{nowUnixTimeStamp},\"domContentLoadedEventStart\":{nowUnixTimeStamp},\"domContentLoadedEventEnd\":{nowUnixTimeStamp},\"domComplete\":{nowUnixTimeStamp},\"loadEventStart\":{nowUnixTimeStamp},\"loadEventEnd\":{nowUnixTimeStamp}}}}},\"end\":{nowUnixTimeStamp},\"timeToSubmit\":108873,\"form\":{{\"email\":{{\"keys\":0,\"keyPressTimeIntervals\":[],\"copies\":0,\"cuts\":0,\"pastes\":0,\"clicks\":0,\"touches\":0,\"mouseClickPositions\":[],\"keyCycles\":[],\"mouseCycles\":[],\"touchCycles\":[],\"width\":290,\"height\":43,\"checksum\":\"C860E86B\",\"time\":12773,\"autocomplete\":false,\"prefilled\":false}},\"password\":{{\"keys\":0,\"keyPressTimeIntervals\":[],\"copies\":0,\"cuts\":0,\"pastes\":0,\"clicks\":0,\"touches\":0,\"mouseClickPositions\":[],\"keyCycles\":[],\"mouseCycles\":[],\"touchCycles\":[],\"width\":290,\"height\":43,\"time\":10353,\"autocomplete\":false,\"prefilled\":false}}}},\"canvas\":{{\"hash\":-373378155,\"emailHash\":-1447130560,\"histogramBins\":[]}},\"token\":null,\"errors\":[],\"metrics\":[{{\"n\":\"fwcim-mercury-collector\",\"t\":0}},{{\"n\":\"fwcim-instant-collector\",\"t\":0}},{{\"n\":\"fwcim-element-telemetry-collector\",\"t\":2}},{{\"n\":\"fwcim-script-version-collector\",\"t\":0}},{{\"n\":\"fwcim-local-storage-identifier-collector\",\"t\":0}},{{\"n\":\"fwcim-timezone-collector\",\"t\":0}},{{\"n\":\"fwcim-script-collector\",\"t\":1}},{{\"n\":\"fwcim-plugin-collector\",\"t\":0}},{{\"n\":\"fwcim-capability-collector\",\"t\":1}},{{\"n\":\"fwcim-browser-collector\",\"t\":0}},{{\"n\":\"fwcim-history-collector\",\"t\":0}},{{\"n\":\"fwcim-gpu-collector\",\"t\":1}},{{\"n\":\"fwcim-battery-collector\",\"t\":0}},{{\"n\":\"fwcim-dnt-collector\",\"t\":0}},{{\"n\":\"fwcim-math-fingerprint-collector\",\"t\":0}},{{\"n\":\"fwcim-performance-collector\",\"t\":0}},{{\"n\":\"fwcim-timer-collector\",\"t\":0}},{{\"n\":\"fwcim-time-to-submit-collector\",\"t\":0}},{{\"n\":\"fwcim-form-input-telemetry-collector\",\"t\":4}},{{\"n\":\"fwcim-canvas-collector\",\"t\":2}},{{\"n\":\"fwcim-captcha-telemetry-collector\",\"t\":0}},{{\"n\":\"fwcim-proof-of-work-collector\",\"t\":1}},{{\"n\":\"fwcim-ubf-collector\",\"t\":0}},{{\"n\":\"fwcim-timer-collector\",\"t\":0}}]}}"
			#endregion
				;
			return raw_metadata;
		}
	}
}
