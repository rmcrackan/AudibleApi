using System;

namespace AudibleApi.Authorization;

/// <summary>
/// Failed to decrypt an encrypted identity token field (crypto failure, missing protector, or OS secret store unavailable).
/// Not a JSON shape error - do not treat as <see cref="Newtonsoft.Json.JsonReaderException"/>.
/// </summary>
public sealed class IdentityTokenDecryptException : Exception
{
	public string FieldName { get; }

	public IdentityTokenDecryptException(string fieldName, Exception innerException)
		: base(BuildMessage(fieldName), innerException)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
		ArgumentNullException.ThrowIfNull(innerException);
		FieldName = fieldName;
	}

	private static string BuildMessage(string fieldName)
		=> $"Failed to decrypt {fieldName}.";
}
