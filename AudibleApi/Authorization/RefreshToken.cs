using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Diagnostics;

namespace AudibleApi.Authorization;

[DebuggerDisplay("{ToString(),nq}")]
public class RefreshToken : StrongType<SecretString>
{
	public const string REQUIRED_BEGINNING = "Atnr|";

	public RefreshToken(SecretString value) : base(value) { }

	protected override void ValidateInput(SecretString value)
	{
		var raw = value.Reveal();
		ArgumentValidator.EnsureNotNull(raw, nameof(value));

		if (!raw.StartsWith(REQUIRED_BEGINNING))
			throw new ArgumentException("Improperly formatted refresh token", nameof(value));
	}

	/// <summary>
	/// The token itself, non-null because the constructor validated it. A method rather than a property so
	/// that reflective logging cannot reach it.
	/// </summary>
	public string Reveal() => Value.Reveal()!;

	public override string ToString() => SecretString.Redact(nameof(RefreshToken), Value.Reveal());
}
