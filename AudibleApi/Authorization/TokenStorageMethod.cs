using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudibleApi.Authorization;

/// <summary>
/// How newly saved or refreshed identity secrets are persisted.
/// When a persisted preference is missing, resolve to <see cref="Encrypted"/>.
/// Unknown future values should not fall back to plaintext.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TokenStorageMethod
{
	/// <summary>Store secrets using authenticated encryption.</summary>
	Encrypted = 0,

	/// <summary>Store secrets as plaintext in the identity JSON.</summary>
	Plaintext = 1
}
