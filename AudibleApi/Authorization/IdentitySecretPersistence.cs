using System.Collections.Generic;

namespace AudibleApi.Authorization;

/// <summary>
/// Tracks whether each secret was loaded encrypted and whether it changed since load,
/// so partial updates can preserve sibling encryption metadata.
/// </summary>
internal sealed class IdentitySecretPersistence
{
	public FieldState AccessToken { get; } = new();
	public FieldState RefreshToken { get; } = new();
	public FieldState AdpToken { get; } = new();
	public FieldState PrivateKey { get; } = new();
	public FieldState StoreAuthenticationCookie { get; } = new();

	public bool CookiesDirty { get; set; }
	public List<bool> CookieLoadedEncrypted { get; } = [];

	public void MarkAllSecretsDirty()
	{
		AccessToken.Dirty = true;
		RefreshToken.Dirty = true;
		AdpToken.Dirty = true;
		PrivateKey.Dirty = true;
		StoreAuthenticationCookie.Dirty = true;
		CookiesDirty = true;
	}

	public void ClearDirtyAfterWrite()
	{
		AccessToken.ClearDirty();
		RefreshToken.ClearDirty();
		AdpToken.ClearDirty();
		PrivateKey.ClearDirty();
		StoreAuthenticationCookie.ClearDirty();
		CookiesDirty = false;
	}

	internal sealed class FieldState
	{
		public bool LoadedEncrypted { get; set; }
		public bool Dirty { get; set; }

		public bool ShouldEncrypt()
			=> IdentityTokenStorage.ShouldEncrypt(Dirty, LoadedEncrypted);

		public void ClearDirty() => Dirty = false;

		public void SetWritten(bool encrypted)
		{
			LoadedEncrypted = encrypted;
			Dirty = false;
		}
	}
}
