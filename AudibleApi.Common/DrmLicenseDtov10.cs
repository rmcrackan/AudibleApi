using Newtonsoft.Json;

namespace AudibleApi.Common;

public class DrmLicenseDtov10 : V10Base<DrmLicenseDtov10>
{
	[JsonProperty("license")]
	public string? License { get; set; }

	[JsonProperty("reason")]
	public string? Reason { get; set; }
}
