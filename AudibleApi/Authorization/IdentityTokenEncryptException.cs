using System;

namespace AudibleApi.Authorization;

/// <summary>
/// Failed to encrypt an identity token field (crypto failure, missing protector, or OS secret store unavailable).
/// Not a JSON serialization shape error - do not treat as <see cref="Newtonsoft.Json.JsonSerializationException"/>.
/// </summary>
public sealed class IdentityTokenEncryptException : Exception
{
	public string FieldName { get; }

	public IdentityTokenEncryptException(string fieldName, Exception innerException)
		: base(BuildMessage(fieldName), innerException)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
		ArgumentNullException.ThrowIfNull(innerException);
		FieldName = fieldName;
	}

	private static string BuildMessage(string fieldName)
		=> $"Failed to encrypt {fieldName}.";
}
