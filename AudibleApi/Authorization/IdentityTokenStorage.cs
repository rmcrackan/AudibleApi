using System;
using Dinah.Core.Security;

namespace AudibleApi.Authorization;

/// <summary>
/// Process-wide configuration for identity token persistence.
/// Host applications (or tests) should call <see cref="Configure"/> at startup.
/// When unconfigured, writes remain plaintext for backward-compatible demos.
/// </summary>
public static class IdentityTokenStorage
{
	private static readonly object Gate = new();
	private static TokenStorageMethod _writeMethod = TokenStorageMethod.Plaintext;
	private static AesGcmSecretProtector? _protector;

	public static TokenStorageMethod WriteMethod
	{
		get { lock (Gate) return _writeMethod; }
	}

	public static AesGcmSecretProtector? Protector
	{
		get { lock (Gate) return _protector; }
	}

	/// <summary>
	/// Configure how identity secrets are written and how encrypted values are decrypted.
	/// </summary>
	public static void Configure(TokenStorageMethod writeMethod, AesGcmSecretProtector? protector)
	{
		lock (Gate)
		{
			_writeMethod = writeMethod;
			_protector = protector;
		}
	}

	/// <summary>Restore the default unconfigured state (plaintext writes, no protector).</summary>
	public static void Reset()
		=> Configure(TokenStorageMethod.Plaintext, protector: null);

	/// <summary>
	/// Run <paramref name="action"/> with a temporary write method, restoring prior configuration afterward.
	/// The current protector is preserved.
	/// </summary>
	public static void RunWithWriteMethod(TokenStorageMethod writeMethod, Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		lock (Gate)
		{
			var previousMethod = _writeMethod;
			var previousProtector = _protector;
			try
			{
				_writeMethod = writeMethod;
				action();
			}
			finally
			{
				_writeMethod = previousMethod;
				_protector = previousProtector;
			}
		}
	}

	internal static bool ShouldEncrypt(bool fieldDirty, bool loadedEncrypted)
		=> fieldDirty
			? WriteMethod == TokenStorageMethod.Encrypted
			: loadedEncrypted;

	internal static string Protect(string plaintext, string associatedData)
	{
		var protector = Protector
			?? throw new InvalidOperationException("Encrypted token storage requires a configured protector.");
		return protector.Protect(plaintext, associatedData);
	}

	internal static string Unprotect(string payload, string associatedData)
	{
		var protector = Protector
			?? throw new InvalidOperationException("Encrypted token requires a configured protector to decrypt.");
		return protector.Unprotect(payload, associatedData);
	}
}
