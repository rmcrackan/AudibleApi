using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;

namespace AudibleApi.Cryptography;

[DebuggerDisplay("{ToString(),nq}")]
public partial class PrivateKey : StrongType<SecretString>, IDisposable
{
	public const string REQUIRED_BEGINNING = "-----BEGIN RSA PRIVATE KEY-----";
	public const string REQUIRED_ENDING = "-----END RSA PRIVATE KEY-----";
	private RSACryptoServiceProvider RSACryptoService;

	public PrivateKey(SecretString value) : base(value)
	{
		if (RSACryptoService is null)
			throw new ArgumentException("Improperly formatted RSA private key", nameof(value));
	}

	public void Dispose()
	{
		RSACryptoService.Dispose();
	}

	/// <summary>
	/// Validates the input RSA private key by importing it into an <see cref="RSACryptoServiceProvider"/>.
	/// A successful validation sets <see cref="RSACryptoService"/>.
	/// </summary>
	/// <remarks>A key cannot be fully validated until it has been successfully imported into the
	/// <see cref="RSACryptoServiceProvider"/>. Since all initialization costs are borne by the validator,
	/// use its work to set <see cref="RSACryptoService"/>.
	/// </remarks>
	/// <param name="value">The RSA private key to validate. Can be either a PEM or base64 DER.</param>
	/// <exception cref="ArgumentException">Thrown when the RSA private key is improperly formatted.</exception>
	protected override void ValidateInput(SecretString value)
	{
		string rawKeyStr
			= ArgumentValidator.EnsureNotNullOrWhiteSpace(value.Reveal(), nameof(value))
			.Trim()
			.Replace("\\r", "")
			.Replace("\\n", "");

		if (PrivateKeyMatch().Match(rawKeyStr) is not { Success: true } match)
			throw new FormatException("Improperly formatted RSA private key");

		Span<byte> rawKeyBts = Encoding.UTF8.GetBytes(match.Groups["b64_der"].Value);
		if (Base64.DecodeFromUtf8InPlace(rawKeyBts, out _) != System.Buffers.OperationStatus.Done)
			throw new FormatException("Improperly formatted RSA private key");

		KeyFormat keyType = DetermineKeyFormat(rawKeyBts);
		if (keyType == KeyFormat.Unknown)
			throw new FormatException("Improperly formatted RSA private key");

		RSACryptoServiceProvider rsa = new();
		if (keyType == KeyFormat.Pkcs1)
		{
			rsa.ImportRSAPrivateKey(rawKeyBts, out _);
		}
		else
		{
			rsa.ImportPkcs8PrivateKey(rawKeyBts, out _);
		}
		RSACryptoService = rsa;
	}

	private enum KeyFormat { Unknown, Pkcs1, Pkcs8 }

	/// <summary>
	/// Tries to determine whether the DER-encoded private key is in PKCS#1 or PKCS#8 format.
	/// <para/>
	/// Both PKCS#1 and PKCS#8 start with a sequence followed by an integer version number.
	/// After the version number, PKCS#1 lists the private key parameters (integers),
	/// while PKCS#8 lists an algorithm identifier (inside a sequence).
	/// </summary>
	/// <remarks>
	/// When AudibleApi only supported iPhone, it stored device private keys in PKCS#1 PEM format.
	/// PKCS#1 PEM files are dentoed with 'BEGIN/END RSA PRIVATE KEY'. When AudibleApi changed its
	/// device emulation to Android, which usses PKCS#8 keys, it erroneously continued storing the
	/// device private kays as a PKCS#1 PEM with the 'BEGIN/END RSA PRIVATE KEY' markers instead
	/// of the correct PCKS#8 markers 'BEGIN/END PRIVATE KEY'. And recently, AudibleApi has been
	/// stripping the PEM markers completely. With the addition of 'Device Profiles', any key
	/// could be either PKCS#1 or PKCS#8. This method is used to determine the actual format of
	/// the key, regardless of the PEM markers.
	/// </remarks>
	private static KeyFormat DetermineKeyFormat(ReadOnlySpan<byte> der)
	{
		if (!AsnDecoder.TryReadEncodedValue(der, AsnEncodingRules.DER, out Asn1Tag tag, out int contentOffset, out _, out _) || tag != Asn1Tag.Sequence)
			return KeyFormat.Unknown;

		der = der[contentOffset..];
		if (!AsnDecoder.TryReadEncodedValue(der, AsnEncodingRules.DER, out tag, out _, out _, out int bytesConsumed) || tag != Asn1Tag.Integer)
			return KeyFormat.Unknown;

		if (!Asn1Tag.TryDecode(der[bytesConsumed..], out tag, out _))
			return KeyFormat.Unknown;

		return tag == Asn1Tag.Integer ? KeyFormat.Pkcs1
			 : tag == Asn1Tag.Sequence ? KeyFormat.Pkcs8
			 : KeyFormat.Unknown;
	}

	public string ExportToPkcs1Pem() => RSACryptoService.ExportRSAPrivateKeyPem();
	public string ExportToPkcs8Pem() => RSACryptoService.ExportPkcs8PrivateKeyPem();

	/// <summary>
	/// The key itself, non-null because the constructor validated it. A method rather than a property so that
	/// reflective logging cannot reach it.
	/// </summary>
	public string Reveal() => Value.Reveal()!;

	public override string ToString() => SecretString.Redact(nameof(PrivateKey), Value.Reveal());

	public string SignMessage(string message)
	{
		var dataBytes = Encoding.UTF8.GetBytes(message);
		var signedBytes = RSACryptoService.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		return Convert.ToBase64String(signedBytes);
	}

	[System.Text.RegularExpressions.GeneratedRegex(@"(-----BEGIN (RSA |)PRIVATE KEY-----)?(?<b64_der>.*)(?(1)-----END \2PRIVATE KEY-----)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.Compiled)]
	private static partial System.Text.RegularExpressions.Regex PrivateKeyMatch();
}
