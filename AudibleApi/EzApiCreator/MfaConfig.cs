using System.Collections.Generic;

namespace AudibleApi;

public record MfaConfigButton
{
	// optional string settings
	public string? Text { get; set; }

	// mandatory values
	public string? Name { get; set; }
	public string? Value { get; set; }
}
public class MfaConfig
{
	// optional
	public string? Title { get; set; }

	public List<MfaConfigButton> Buttons { get; } = new();
}
