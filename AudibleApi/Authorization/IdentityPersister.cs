using Dinah.Core.IO;
using Newtonsoft.Json;

namespace AudibleApi.Authorization;

public class IdentityPersister : JsonFilePersister<Identity>
{
	/// <summary>Alias for Target </summary>
	public Identity Identity => Target;

	/// <summary>uses path. create file if doesn't yet exist</summary>
	public IdentityPersister(Identity identity, string path, string? jsonPath = null)
		: base(identity, path, jsonPath) { }

	/// <summary>load from existing file</summary>
	public IdentityPersister(string path, string? jsonPath = null)
		: base(path, jsonPath) { }

	protected override JsonSerializerSettings GetSerializerSettings()
		=> Identity.GetJsonSerializerSettings();
}
