using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AudibleApi.Cryptography;

/// <summary>
/// Encode and decode the amazon frc cookie
/// </summary>
internal class FrcEncoder
{
	public static string Encode(string deviceSn, string json)
	{
		var compressed = GzipCompress(Encoding.UTF8.GetBytes(json));
		var key = deviceSn.AsSpan();
		ReadOnlySpan<byte> iv = RandomNumberGenerator.GetBytes(16);

		var encdypted = EncryptFrc(key, iv, compressed);
		var sig = ComputeSig(key, iv, encdypted);
		var bts = new byte[1 + sig.Length + iv.Length + encdypted.Length];
		sig.CopyTo(bts.AsSpan(1));
		iv.CopyTo(bts.AsSpan(1 + sig.Length));
		encdypted.CopyTo(bts.AsSpan(1 + sig.Length + iv.Length));

		return Convert.ToBase64String(bts);
	}

	public static string Decode(string deviceSn, string encoded)
	{
		ReadOnlySpan<byte> bytes = Convert.FromBase64String(encoded.Trim());
		var key = deviceSn.AsSpan();
		var sig = bytes[1..9];
		var iv = bytes[9..25];
		var data = bytes[25..];

		if (!sig.SequenceEqual(ComputeSig(key, iv, data)))
			throw new InvalidDataException("Invalid signature");
		var decData = DecryptFrc(key, iv, data);
		var decompData = GzipDecompress(decData);
		return Encoding.UTF8.GetString(decompData);
	}
	private static byte[] GzipCompress(byte[] data)
	{
		using var ms2 = new MemoryStream();
		using (var decomp = new GZipStream(ms2, CompressionLevel.SmallestSize))
			decomp.Write(data);
		return ms2.ToArray();
	}
	private static byte[] GzipDecompress(byte[] data)
	{
		using var ms = new MemoryStream(data);
		using var ms2 = new MemoryStream();
		using var decomp = new GZipStream(ms, CompressionMode.Decompress);
		decomp.CopyTo(ms2);
		return ms2.ToArray();
	}
	private static byte[] EncryptFrc(ReadOnlySpan<char> deviceSn, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> data)
	{
		using var aes = GetAes(deviceSn);
		return aes.EncryptCbc(data, iv, PaddingMode.PKCS7);
	}
	private static byte[] DecryptFrc(ReadOnlySpan<char> deviceSn, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> data)
	{
		using var aes = GetAes(deviceSn);
		return aes.DecryptCbc(data, iv, PaddingMode.PKCS7);
	}
	private static byte[] ComputeSig(ReadOnlySpan<char> deviceSn, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> data)
	{
		var key = GetKeyFromPassword(deviceSn, "HmacSHA256"u8);
		using var hmac = new HMACSHA256(key);
		var btes = new byte[iv.Length + data.Length];
		iv.CopyTo(btes);
		data.CopyTo(btes.AsSpan(iv.Length));
		return hmac.ComputeHash(btes)[..8];
	}
	private static Aes GetAes(ReadOnlySpan<char> deviceSn)
	{
		var aes = Aes.Create();
		aes.Key = GetKeyFromPassword(deviceSn, "AES/CBC/PKCS7Padding"u8);
		return aes;
	}

	private static byte[] GetKeyFromPassword(ReadOnlySpan<char> deviceSn, ReadOnlySpan<byte> salt)
		=> Rfc2898DeriveBytes.Pbkdf2(deviceSn, salt, 1000, HashAlgorithmName.SHA1, 16);
}
