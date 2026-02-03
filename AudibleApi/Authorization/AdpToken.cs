using Dinah.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudibleApi.Authorization;

public class AdpToken : StrongType<string>
{
	public AdpToken(string value) : base(value) { }

	protected override void ValidateInput(string? value)
	{
		ArgumentValidator.EnsureNotNull(value, "value");

		var ex = new ArgumentException("Improperly formatted ADP token");

		if (!value.StartsWith("{"))
			throw ex;

		if (!value.EndsWith("}"))
			throw ex;

		var dic = adp_parser.Parse(value);

		if (dic.Count != 5) throw ex;

		if (!dic.ContainsKey("enc")) throw ex;
		if (!dic.ContainsKey("key")) throw ex;
		if (!dic.ContainsKey("iv")) throw ex;
		if (!dic.ContainsKey("name")) throw ex;
		if (!dic.ContainsKey("serial")) throw ex;

		// QURQVG9rZW5FbmNyeXB0aW9uS2V5 is base64 encode of "ADPTokenEncryptionKey"
		if (dic["name"] != "QURQVG9rZW5FbmNyeXB0aW9uS2V5") throw ex;

		// serial seems to always be "Mg==" which is base64 encode
		// of "2" but no reason this is necessary
	}

	public static class adp_parser
	{
		static bool isBase64Char(char c) =>
			char.IsLetter(c) ||
			char.IsNumber(c) ||
			c == '+' ||
			c == '/' ||
			c == '=';

		enum State { preKey, key, val }
		public static Dictionary<string, string> Parse(string input)
		{
			var dic = new Dictionary<string, string>();

			var keyBuilder = new StringBuilder();
			var valBuilder = new StringBuilder();
			var s = State.preKey;
			string? currKey = null;

			var ex = new ArgumentException("Improperly formatted ADP token");

			foreach (var c in input)
			{
				switch (s)
				{
					case State.preKey:
						if (c == '{')
							s = State.key;
						else
							throw new Exception();
						break;
					case State.key:
						if (c == ':')
						{
							currKey = keyBuilder.ToString();
							keyBuilder.Clear();
							s = State.val;
						}
						else if (char.IsLetter(c))
							keyBuilder.Append(c);
						else
							throw ex;
						break;
					case State.val:
						if (c == '}')
						{
							if (currKey is null)
								throw new InvalidDataException("Failed to parse ADP Token");
							dic[currKey] = valBuilder.ToString();
							valBuilder.Clear();
							currKey = null;
							s = State.preKey;
						}
						else if (isBase64Char(c))
							valBuilder.Append(c);
						else
							throw ex;
						break;
				}
			}

			if (s != State.preKey)
				throw ex;

			return dic;
		}
	}
}
