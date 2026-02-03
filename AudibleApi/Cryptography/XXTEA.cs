using Dinah.Core;
using System;
using System.Runtime.InteropServices;

namespace AudibleApi.Cryptography;

/// <summary>
/// Corrected Block TEA 
/// </summary>
/// <remarks>https://en.wikipedia.org/wiki/XXTEA#Reference_code</remarks>
public static class XXTEA
{
	public static byte[] Encrypt(ReadOnlySpan<byte> clearBytes, uint[] key)
	{
		ArgumentValidator.EnsureNotNull(key, nameof(key));
		ArgumentValidator.EnsureBetweenInclusive(key.Length, nameof(key), 4, 4);

		int n = (int)Math.Ceiling(clearBytes.Length / 4d);

		byte[] cipherBytes = new byte[n * sizeof(uint)];
		Span<uint> transformBuffer = MemoryMarshal.Cast<byte, uint>(cipherBytes.AsSpan());
		clearBytes.CopyTo(cipherBytes);

		Transform(transformBuffer, key, encrypting: true);

		return cipherBytes;
	}

	public static byte[] Decrypt(ReadOnlySpan<byte> cipherBytes, uint[] key)
	{
		ArgumentValidator.EnsureNotNull(key, nameof(key));
		ArgumentValidator.EnsureBetweenInclusive(key.Length, nameof(key), 4, 4);

		int n = (int)Math.Ceiling(cipherBytes.Length / 4d);

		byte[] clearBytes = new byte[n * sizeof(uint)];
		Span<uint> transformBuffer = MemoryMarshal.Cast<byte, uint>(clearBytes.AsSpan());
		cipherBytes.CopyTo(clearBytes);

		Transform(transformBuffer, key, encrypting: false);

		return clearBytes;
	}

	private static void Transform(Span<uint> v, uint[] key, bool encrypting)
	{
		const uint DELTA = 0x9e3779b9;
		int p, n = v.Length, rounds = 6 + 52 / n;
		uint z, y, sum, e;

		uint MX() => (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);

		if (encrypting)
		{
			sum = 0;
			z = v[n - 1];

			for (; rounds > 0; rounds--)
			{
				sum += DELTA;
				e = sum >> 2 & 3;

				for (p = 0; p < n - 1; p++)
				{
					y = v[p + 1];
					z = v[p] += MX();
				}

				y = v[0];
				z = v[^1] += MX();
			}
		}
		else
		{
			sum = (uint)rounds * DELTA;
			y = v[0];

			for (; rounds > 0; rounds--)
			{
				e = sum >> 2 & 3;

				for (p = n - 1; p > 0; p--)
				{
					z = v[p - 1];
					y = v[p] -= MX();
				}

				z = v[^1];
				y = v[0] -= MX();

				sum -= DELTA;
			}
		}
	}
}
