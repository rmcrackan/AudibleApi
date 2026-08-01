using Dinah.Core;
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

	public override string ToString()
		=> Value is null
			? "RefreshToken [REDACTED <null>]"
			: $"RefreshToken [REDACTED length={Value.Length}]";
}
