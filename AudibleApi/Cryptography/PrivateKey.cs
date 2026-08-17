using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AudibleApi.Cryptography;

[DebuggerDisplay("{ToString(),nq}")]
public class PrivateKey : StrongType<SecretString>
{
	public const string REQUIRED_BEGINNING = "-----BEGIN RSA PRIVATE KEY-----";
	public const string REQUIRED_ENDING = "-----END RSA PRIVATE KEY-----";
	private readonly RSACryptoServiceProvider RSACryptoService;

	public PrivateKey(SecretString value) : base(value)
	{
		RSACryptoService = CreateRsaProviderFromPrivateKey(value.Reveal()!);
	}

	protected override void ValidateInput(SecretString value)
	{
		var raw = value.Reveal();
		ArgumentValidator.EnsureNotNullOrWhiteSpace(raw, nameof(value));

		if (Convert.TryFromBase64String(raw.Trim(), new byte[raw.Length], out _))
			return;

		if (!raw.Trim().StartsWith(REQUIRED_BEGINNING))
			throw new ArgumentException("Improperly formatted RSA private key", nameof(value));

		if (!raw.Trim().EndsWith(REQUIRED_ENDING))
			throw new ArgumentException("Improperly formatted RSA private key", nameof(value));
	}

	/// <summary>
	/// The key itself, non-null because the constructor validated it. A method rather than a property so that
	/// reflective logging cannot reach it.
	/// </summary>
	public string Reveal() => Value.Reveal()!;

	public override string ToString() => SecretString.Redact(nameof(PrivateKey), Value.Reveal());

	public string SignMessage(string message)
	{
		var dataBytes = Encoding.UTF8.GetBytes(message);

		var digestion = SHA256.HashData(dataBytes);
		var signedBytes = RSACryptoService.SignHash(digestion, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		return Convert.ToBase64String(signedBytes);
	}

	// https://stackoverflow.com/a/32150537
	private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
	{
		privateKey = privateKey
			.Replace(REQUIRED_BEGINNING, "")
			.Replace(REQUIRED_ENDING, "")
			.Replace("\\n", "")
			.Trim();

		var asn1EncodedPrivateKey = Convert.FromBase64String(privateKey);
		var RSA = new RSACryptoServiceProvider();
		try
		{
			//Try importing the iPhone private key, which is PKCS #1.
			RSA.ImportRSAPrivateKey(asn1EncodedPrivateKey, out _);
		}
		catch (CryptographicException)
		{
			//Fallback to importing the Android private key, which is PKCS#8.
			RSA.ImportPkcs8PrivateKey(asn1EncodedPrivateKey, out _);
		}
		return RSA;
	}
}
