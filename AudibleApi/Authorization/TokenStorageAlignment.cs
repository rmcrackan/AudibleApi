namespace AudibleApi.Authorization;

/// <summary>
/// How stored identity secrets compare to a requested <see cref="TokenStorageMethod"/>.
/// </summary>
public enum TokenStorageAlignment
{
	/// <summary>Every applicable secret already matches the requested method.</summary>
	AllMatch = 0,

	/// <summary>At least one applicable secret does not match the requested method.</summary>
	SomeMismatch = 1,

	/// <summary>No applicable secrets are present.</summary>
	NoApplicableTokens = 2,

	/// <summary>Alignment could not be determined (for example a read/decrypt failure).</summary>
	Indeterminate = 3
}
