using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AudibleApi.Authorization;

[DebuggerDisplay("{ToString(),nq}")]
public class AdpToken : StrongType<SecretString>
{
	public AdpToken(SecretString value) : base(value) { }

	protected override void ValidateInput(SecretString value)
	{
		var raw = value.Reveal();
		ArgumentValidator.EnsureNotNull(raw, "value");

		var ex = new ArgumentException("Improperly formatted ADP token");

		if (!raw.StartsWith("{"))
			throw ex;

		if (!raw.EndsWith("}"))
			throw ex;

		var dic = adp_parser.Parse(raw);

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

	/// <summary>
	/// The token itself, non-null because the constructor validated it. A method rather than a property so
	/// that reflective logging cannot reach it.
	/// </summary>
	public string Reveal() => Value.Reveal()!;

	public override string ToString() => SecretString.Redact(nameof(AdpToken), Value.Reveal());

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
