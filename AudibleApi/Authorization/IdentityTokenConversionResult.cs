using System.Collections.Generic;

namespace AudibleApi.Authorization;

/// <summary>
/// Result of an identity token conversion attempt. Messages must never include secret values.
/// </summary>
public sealed class IdentityTokenConversionResult
{
	public bool Succeeded { get; init; }
	public bool Changed { get; init; }
	public string? BackupPath { get; init; }
	public TokenStorageAlignment AlignmentBefore { get; init; }
	public string? Error { get; init; }
	public IReadOnlyList<string> FailedCategories { get; init; } = [];

	public static IdentityTokenConversionResult NoChanges(TokenStorageAlignment alignmentBefore)
		=> new()
		{
			Succeeded = true,
			Changed = false,
			AlignmentBefore = alignmentBefore
		};

	public static IdentityTokenConversionResult Success(TokenStorageAlignment alignmentBefore, string? backupPath)
		=> new()
		{
			Succeeded = true,
			Changed = true,
			AlignmentBefore = alignmentBefore,
			BackupPath = backupPath
		};

	public static IdentityTokenConversionResult Failure(TokenStorageAlignment alignmentBefore, string error, params string[] failedCategories)
		=> new()
		{
			Succeeded = false,
			Changed = false,
			AlignmentBefore = alignmentBefore,
			Error = error,
			FailedCategories = failedCategories
		};
}
