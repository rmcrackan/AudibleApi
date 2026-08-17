using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Diagnostics;

namespace AudibleApi.Authorization;

[DebuggerDisplay("{ToString(),nq}")]
public class RefreshToken : StrongType<string>
{
	public const string REQUIRED_BEGINNING = "Atnr|";

	public RefreshToken(string value) : base(value) { }

	protected override void ValidateInput(string? value)
	{
		ArgumentValidator.EnsureNotNull(value, nameof(value));

		if (!value.StartsWith(REQUIRED_BEGINNING))
			throw new ArgumentException("Improperly formatted refresh token", nameof(value));
	}

	public override string ToString() => SecretString.Redact(nameof(RefreshToken), Value);
}
