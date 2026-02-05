using Dinah.Core;
using System;

namespace AudibleApi.Authorization;

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
}
